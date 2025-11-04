using System;
using System.Threading.Tasks;

namespace MainMenuControlViewModel
{
    using CUIServices;
    using LogModule;
    using Pranas;
    using ProberErrorCode;
    using ProberInterfaces;
    using ProberInterfaces.DialogControl;
    using RelayCommandBase;
    using System.ComponentModel;
    using System.IO;
    using System.Windows;
    using System.Windows.Input;
    //using ThreadLockExplorerDialog;
    using UCTaskManagement;
    using DBManagerModule;
    using System.Runtime.CompilerServices;
    using ProberInterfaces.LoaderController;
    using ProberInterfaces.ProberSystem;
    using MetroDialogInterfaces;

    public class MainMenuVM : INotifyPropertyChanged, IFactoryModule
    {

        //public event PropertyChangedEventHandler PropertyChanged;
        //private void NotifyPropertyChanged([CallerMemberName]String info = null)
        //    => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));

        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        public MainMenuVM()
        {
        }

        public bool Initialized { get; set; } = false;

        readonly Guid _ViewModelGUID = new Guid("cb944761-9e00-4787-b760-e584bb5c380a");
        readonly string HomeGuidString = "6223DFD5-EFAA-4B49-AB70-D8A5F03FA65D";
        public Guid ScreenGUID { get { return _ViewModelGUID; } }

        //private ProberViewModel _ProbeView;
        //IProberMainScreen _ProberMain;

        //public MainMenuVM(ProberViewModel probeView)
        //{
        //    _ProbeView = probeView;
        //}

        private IViewModelManager _ViewModelManager;
        public IViewModelManager ViewModelManager
        {
            get { return _ViewModelManager; }
            set
            {
                if (value != _ViewModelManager)
                {
                    _ViewModelManager = value;
                    RaisePropertyChanged();
                }
            }
        }
        public ILotOPModule LotOPModule { get; set; }
        public IMonitoringManager MonitoringManager { get; set; }
        public ILoaderController LoaderController { get; set; }

        #region ..//Command & CommandMethod

        #region ..//LOTMENU

        private AsyncCommand _GoHomeScreen;
        public ICommand GoHomeScreen
        {
            get
            {
                if (null == _GoHomeScreen) _GoHomeScreen = new AsyncCommand(HomeScreen);
                return _GoHomeScreen;
            }
        }

        private Task HomeScreen()
        {
            this.ViewModelManager().ViewTransitionAsync(new Guid(HomeGuidString));

            return Task.FromResult<object>(null);
        }

        /// <summary>
        /// Cell이 Widget으로 전환될 때 Home화면으로 전환해 놓는다.
        /// </summary>
        public async void Go_HomeScreen()
        {
            try
            {
                await this.ViewModelManager().ViewTransitionAsync(new Guid(HomeGuidString));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }            
        }

        private AsyncCommand<object> _GoPrevScreen;
        public ICommand GoPrevScreen
        {
            get
            {
                if (null == _GoPrevScreen) _GoPrevScreen = new AsyncCommand<object>(PrevScreen);
                return _GoPrevScreen;
            }
        }

        private async Task PrevScreen(object noparam)
        {
            try
            {
                await this.ViewModelManager().BackPreScreenTransition();
                //_ProbeView.BackPreScreenTransition();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        #endregion

        #region ..//OPERATION

        private RelayCommand<CUI.MenuItem> _DoProbeCardChangeCommand;
        public ICommand DoProbeCardChangeCommand
        {
            get
            {
                if (null == _DoProbeCardChangeCommand) _DoProbeCardChangeCommand = new RelayCommand<CUI.MenuItem>(DoProbeCardChange);
                return _DoProbeCardChangeCommand;
            }
        }
        private void DoProbeCardChange(CUI.MenuItem cuiparam)
        {
            try
            {
                Guid ViewGUID = CUIService.GetTargetViewGUID(cuiparam.GUID);
                this.ViewModelManager().ViewTransitionAsync(ViewGUID);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        #endregion

        #region ..//SETUP
        private RelayCommand<CUI.MenuItem> _DoSetUpRecipe;
        public ICommand DoSetUpRecipe
        {
            get
            {
                if (null == _DoSetUpRecipe) _DoSetUpRecipe = new RelayCommand<CUI.MenuItem>(DoRecipeSetup);
                return _DoSetUpRecipe;
            }
        }
        private void DoRecipeSetup(CUI.MenuItem cuiparam)
        {
            try
            {
                Guid ViewGUID = CUIService.GetTargetViewGUID(cuiparam.GUID);
                this.ViewModelManager().ViewTransitionAsync(ViewGUID);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand<object> _DoHBChiller;
        public ICommand DoHBChiller
        {
            get
            {
                if (null == _DoHBChiller) _DoHBChiller = new RelayCommand<object>(DoHBChillerSetup);
                return _DoHBChiller;
            }
        }
        private void DoHBChillerSetup(object noparam)
        {
            try
            {

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        #endregion

        #region ..//RECIPE
        #endregion

        #region ..//SYSTEN

        private AsyncCommand<CUI.MenuItem> _ManualProbePageSwitchingCommand;
        public ICommand ManualProbePageSwitchingCommand
        {
            get
            {
                if (null == _ManualProbePageSwitchingCommand) _ManualProbePageSwitchingCommand = new AsyncCommand<CUI.MenuItem>(FuncManualProbePageSwitchingCommand);
                return _ManualProbePageSwitchingCommand;
            }
        }
        private async Task FuncManualProbePageSwitchingCommand(CUI.MenuItem cuiparam)
        {
            try
            {
                bool WaferAligned = false;
                bool PinAligned = false;

                string waferdatavalid = string.Empty;
                string pindatavalid = string.Empty;

                if (this.StageSupervisor().WaferObject.GetAlignState() == AlignStateEnum.DONE)
                {
                    WaferAligned = true;
                    waferdatavalid = "is enough.";
                }
                else
                {
                    waferdatavalid = "is not enough.";
                }

                if (this.StageSupervisor().ProbeCardInfo.GetAlignState() == AlignStateEnum.DONE)
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
                    Guid ViewGUID = CUIService.GetTargetViewGUID(cuiparam.GUID);
                    await this.ViewModelManager().ViewTransitionAsync(ViewGUID);
                }
                else
                {
                    await this.MetroDialogManager().ShowMessageDialog("[Information]",
                        $"There is not enough data required.\nWafer Align data {waferdatavalid}\nPin Align data {pindatavalid}"
                        , EnumMessageStyle.Affirmative);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }


        private AsyncCommand<CUI.MenuItem> _PageSwitchingCommand;
        public ICommand PageSwitchingCommand
        {
            get
            {
                if (null == _PageSwitchingCommand) _PageSwitchingCommand = new AsyncCommand<CUI.MenuItem>(FuncPageSwitchingCommand);
                return _PageSwitchingCommand;
            }
        }
        private async Task FuncPageSwitchingCommand(CUI.MenuItem cuiparam)
        {
            try
            {
                Guid ViewGUID = CUIService.GetTargetViewGUID(cuiparam.GUID);
                //this.ViewModelManager().ViewTransition(ViewGUID);
                await this.ViewModelManager().ViewTransitionAsync(ViewGUID);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private AsyncCommand<CUI.MenuItem> _LogOutCommand;
        public ICommand LogOutCommand
        {
            get
            {
                if (null == _LogOutCommand) _LogOutCommand = new AsyncCommand<CUI.MenuItem>(LogOutCmd);
                return _LogOutCommand;
            }
        }
        private async Task LogOutCmd(CUI.MenuItem cuiparam)
        {
            try
            {

                EnumMessageDialogResult ret;

                EventCodeEnum retval = EventCodeEnum.UNDEFINED;

                ret = await this.MetroDialogManager().ShowMessageDialog("Logout", "Are you sure you want to logout?", EnumMessageStyle.AffirmativeAndNegative);

                if (ret == EnumMessageDialogResult.AFFIRMATIVE)
                {
                    Guid ViewGUID = CUIService.GetTargetViewGUID(cuiparam.GUID);
                    await this.ViewModelManager().ViewTransitionAsync(ViewGUID);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        private AsyncCommand<CUI.MenuItem> _ManualSoakingCommand;
        public ICommand ManualSoakingCommand
        {
            get
            {
                if (null == _ManualSoakingCommand) _ManualSoakingCommand = new AsyncCommand<CUI.MenuItem>(ManualSoakingCommandCmd);
                return _ManualSoakingCommand;
            }
        }
        private async Task ManualSoakingCommandCmd(CUI.MenuItem cuiparam)
        {
            try
            {
                EventCodeEnum retval = EventCodeEnum.UNDEFINED;
                Guid ViewGUID = CUIService.GetTargetViewGUID(cuiparam.GUID);
                await this.ViewModelManager().ViewTransitionAsync(ViewGUID);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        private AsyncCommand<CUI.MenuItem> _WaferAlignDialogCommand;
        public ICommand WaferAlignDialogCommand
        {
            get
            {
                if (null == _WaferAlignDialogCommand) _WaferAlignDialogCommand = new AsyncCommand<CUI.MenuItem>(FuncWaferAlignDialogCommand);
                return _WaferAlignDialogCommand;
            }
        }

        private async Task FuncWaferAlignDialogCommand(CUI.MenuItem cuiparam)
        {
            try
            {
                //var AlignDialog = DisplayPortDialogView.GetInstance();
                //System.Windows.Application.Current.Dispatcher.BeginInvoke((Action)(async () =>
                //{
                //    IDisplayPortDialog dialogManager = this.DisplayPortDialog();
                //    dialogManager.WaferAlignOnToggle = true;
                //    dialogManager.PinAlignOnToggle = false;
                //    await dialogManager.ShowDialog();
                    
                //}));

                IDisplayPortDialog dialogManager = this.DisplayPortDialog();
                dialogManager.WaferAlignOnToggle = true;
                dialogManager.PinAlignOnToggle = false;
                await dialogManager.ShowDialog();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                //this.ViewModelManager().UnLock(GetHashCode());
            }
        }

        private AsyncCommand<CUI.MenuItem> _PinAlignDialogCommand;
        public ICommand PinAlignDialogCommand
        {
            get
            {
                if (null == _PinAlignDialogCommand) _PinAlignDialogCommand = new AsyncCommand<CUI.MenuItem>(FuncPinAlignDialogCommand);
                return _PinAlignDialogCommand;
            }
        }

        private async Task FuncPinAlignDialogCommand(CUI.MenuItem cuiparam)
        {
            try
            {
                //var AlignDialog = DisplayPortDialogView.GetInstance();
                //System.Windows.Application.Current.Dispatcher.BeginInvoke((Action)(async () =>
                //{

                //    IDisplayPortDialog dialogManager = this.DisplayPortDialog();
                //    dialogManager.WaferAlignOnToggle = false;
                //    dialogManager.PinAlignOnToggle = true;
                //    await dialogManager.ShowDialog();
                //}));

                //await System.Windows.Application.Current.Dispatcher.Invoke<Task>(() =>
                //{
                IDisplayPortDialog dialogManager = this.DisplayPortDialog();
                dialogManager.WaferAlignOnToggle = false;
                dialogManager.PinAlignOnToggle = true;
                await dialogManager.ShowDialog();
                //});

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        private AsyncCommand<CUI.MenuItem> _TaskManageSwitchingCommand;
        public ICommand TaskManageSwitchingCommand
        {
            get
            {
                if (null == _TaskManageSwitchingCommand) _TaskManageSwitchingCommand = new AsyncCommand<CUI.MenuItem>(FuncTaskManageSwitchingCommand);
                return _TaskManageSwitchingCommand;
            }
        }
        Window TaskView = null;
        private async Task FuncTaskManageSwitchingCommand(CUI.MenuItem cuiparam)
        {
            try
            {
                System.Windows.Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                {
                    if (TaskView != null && TaskView.Visibility == Visibility.Visible)
                    {
                        TaskView.Activate();
                        return;
                    }

                    //var taskManager = await Task.Run(() => TaskManagerDialog.GetInstance());
                    var taskManager = TaskManagerDialog.GetInstance();
                    TaskView = taskManager;
                    TaskView.WindowStyle = WindowStyle.ToolWindow;
                    TaskView.Visibility = Visibility.Visible;
                    // graphSingle.Owner = Model.ProberMain;
                    // OPUS3DView.Closed += (o, args) => OPUS3DView = null;
                    TaskView.Show();
                }));

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand<CUI.MenuItem> _GoWaferHandlingCommand;
        public ICommand GoWaferHandlingCommand
        {
            get
            {
                if (null == _GoWaferHandlingCommand) _GoWaferHandlingCommand = new RelayCommand<CUI.MenuItem>(GoWaferHandling);
                return _GoWaferHandlingCommand;
            }
        }
        private void GoWaferHandling(CUI.MenuItem cuiparam)
        {
            try
            {
                Guid ViewGUID = CUIService.GetTargetViewGUID(cuiparam.GUID);
                this.ViewModelManager().ViewTransitionAsync(ViewGUID);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand<CUI.MenuItem> _StatisticsViewCommand;
        public ICommand StatisticsViewCommand
        {
            get
            {
                if (null == _StatisticsViewCommand) _StatisticsViewCommand = new RelayCommand<CUI.MenuItem>(StatisticsViewCommandFunc);
                return _StatisticsViewCommand;
            }
        }
        private void StatisticsViewCommandFunc(CUI.MenuItem cuiparam)
        {
            try
            {
                Guid ViewGUID = CUIService.GetTargetViewGUID(cuiparam.GUID);
                this.ViewModelManager().ViewTransitionAsync(ViewGUID);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand<CUI.MenuItem> _GoIOControlCommand;
        public ICommand GoIOControlCommand
        {
            get
            {
                if (null == _GoIOControlCommand) _GoIOControlCommand = new RelayCommand<CUI.MenuItem>(GoIOControl);
                return _GoIOControlCommand;
            }
        }
        private void GoIOControl(CUI.MenuItem cuiparam)
        {
            try
            {
                Guid ViewGUID = CUIService.GetTargetViewGUID(cuiparam.GUID);
                this.ViewModelManager().ViewTransitionAsync(ViewGUID);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand<CUI.MenuItem> _GoFoupControlCommand;
        public ICommand GoFoupControlCommand
        {
            get
            {
                if (null == _GoFoupControlCommand) _GoFoupControlCommand = new RelayCommand<CUI.MenuItem>(GoFoupControl);
                return _GoFoupControlCommand;
            }
        }
        private void GoFoupControl(CUI.MenuItem cuiparam)
        {
            try
            {
                Guid ViewGUID = CUIService.GetTargetViewGUID(cuiparam.GUID);
                this.ViewModelManager().ViewTransitionAsync(ViewGUID);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand<object> _GoMotionControlCommand;
        public ICommand GoMotionControlCommand
        {
            get
            {
                if (null == _GoMotionControlCommand) _GoMotionControlCommand = new RelayCommand<object>(GoMotionControl);
                return _GoMotionControlCommand;
            }
        }
        private void GoMotionControl(object noparam)
        {
            try
            {

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand<CUI.MenuItem> _GoTaskManagement;
        public ICommand GoTaskManagement
        {
            get
            {
                if (null == _GoTaskManagement) _GoTaskManagement = new RelayCommand<CUI.MenuItem>(DoTaskManagement);
                return _GoTaskManagement;
            }
        }
        private void DoTaskManagement(CUI.MenuItem cuiparam)
        {
            try
            {
                Guid ViewGUID = CUIService.GetTargetViewGUID(cuiparam.GUID);
                this.ViewModelManager().ViewTransitionAsync(ViewGUID);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }


        private AsyncCommand _SystemInitCommand;
        public ICommand SystemInitCommand
        {
            get
            {
                if (null == _SystemInitCommand) _SystemInitCommand = new AsyncCommand(SystemInit);
                return _SystemInitCommand;
            }
        }



        private async Task SystemInit()
        {
            try
            {
                ////Code block#1//call thread

                //LoggerManager.Debug($"Start Time = {DateTime.Now.ToString()}");

                //var ret = await this.ViewModelManager().Prober._StageSuperVisor.SystemInit(); ////Code bloc#2 - //workqueue task threand

                //LoggerManager.Debug($"End Time = {DateTime.Now.ToString()}");

                ////Code block#3//call thread

                EnumMessageDialogResult ret;

                //EventCodeEnum retval = EventCodeEnum.UNDEFINED;
                ErrorCodeResult retval = new ErrorCodeResult();

                //LotOPModule = this.LotOPModule();
                //LoaderController = this.LoaderController();
                //MonitoringManager = this.MonitoringManager();

                if (this.SequenceEngineManager().GetMovingState() == true)
                {
                    ret = await this.MetroDialogManager().ShowMessageDialog("System Initialize", "Are you sure you want to System Initialize?", EnumMessageStyle.AffirmativeAndNegative);

                    if (ret == EnumMessageDialogResult.AFFIRMATIVE)
                    {
                        LoggerManager.Debug($"Start SystemInit");

                        retval = await this.StageSupervisor().SystemInit();

                        if (retval.ErrorCode != EventCodeEnum.NONE)
                        {
 
                            LoggerManager.Prolog(PrologType.INFORMATION, retval.ErrorCode);
                            this.MetroDialogManager().ShowMessageDialog("System Initialize", $"Failed, Reason = {retval.ErrorCode.ToString()}", EnumMessageStyle.Affirmative);

                            LoggerManager.Debug($"End SystemInit");
                        }
                    }
                }
                else
                {
                    ret = await this.MetroDialogManager().ShowMessageDialog("System Initialize", "Can not Initialize.", EnumMessageStyle.Affirmative);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }



        private AsyncCommand _LoaderInitCommand;
        public ICommand LoaderInitCommand
        {
            get
            {
                if (null == _LoaderInitCommand) _LoaderInitCommand = new AsyncCommand(LoaderInit);
                return _LoaderInitCommand;
            }
        }



        private async Task LoaderInit()
        {
            try
            {
                ////Code block#1//call thread

                //LoggerManager.Debug($"Start Time = {DateTime.Now.ToString()}");

                //var ret = await this.ViewModelManager().Prober._StageSuperVisor.SystemInit(); ////Code bloc#2 - //workqueue task threand

                //LoggerManager.Debug($"End Time = {DateTime.Now.ToString()}");

                ////Code block#3//call thread

                EnumMessageDialogResult ret;

                EventCodeEnum retval = EventCodeEnum.UNDEFINED;

                if (this.SequenceEngineManager().GetMovingState() == true)
                {
                    ret = await this.MetroDialogManager().ShowMessageDialog("Loader Initialize", "Are you sure you want to Loader Initialize?", EnumMessageStyle.AffirmativeAndNegative);

                    if (ret == EnumMessageDialogResult.AFFIRMATIVE)
                    {
                        LoggerManager.Debug($"Start Time = {DateTime.Now.ToString()}");

                        //retval = this.LoaderController().LoaderSystemInit();
                        retval = await this.StageSupervisor().LoaderInit();

                        if (retval != EventCodeEnum.NONE)
                        {
                            this.MetroDialogManager().ShowMessageDialog("Loader Initialize", $"Failed, Reason = {retval}", EnumMessageStyle.Affirmative);
                        }

                        LoggerManager.Debug($"End Time = {DateTime.Now.ToString()}");
                    }
                }
                else
                {
                    ret = await this.MetroDialogManager().ShowMessageDialog("Loader Init", "Can not quit.", EnumMessageStyle.Affirmative);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private AsyncCommand _ClearParameterTableCommand;
        public ICommand ClearParameterTableCommand
        {
            get
            {
                if (null == _ClearParameterTableCommand) _ClearParameterTableCommand = new AsyncCommand(ClearParameterTableCommandFunc);
                return _ClearParameterTableCommand;
            }
        }

        private async Task ClearParameterTableCommandFunc()
        {
            EnumMessageDialogResult ret = await this.MetroDialogManager().ShowMessageDialog("Clear Pramter Table", "Are you sure you want to Drop all Parameter Table?", EnumMessageStyle.AffirmativeAndNegative);

            if (ret == EnumMessageDialogResult.AFFIRMATIVE)
            {
                DBManager.SystemParameter.DropTable();
                DBManager.DeviceParameter.DropTable();
                DBManager.CommonParameter.DropTable();
            }
        }

        private AsyncCommand _ReLoadAltParamCommand;
        public ICommand ReLoadAltParamCommand
        {
            get
            {
                if (null == _ReLoadAltParamCommand) _ReLoadAltParamCommand = new AsyncCommand(ReLoadAltParamCommandFunc);
                return _ReLoadAltParamCommand;
            }
        }

        private async Task ReLoadAltParamCommandFunc()
        {
            EnumMessageDialogResult ret = await this.MetroDialogManager().ShowMessageDialog("Information Message", "Are you sure you want to re-load alt system parameter?", EnumMessageStyle.AffirmativeAndNegative);

            if (ret == EnumMessageDialogResult.AFFIRMATIVE)
            {
                this.ParamManager().LoadSysParameter();
                this.ParamManager().ApplyAltParamToElement();
            }
        }

        private AsyncCommand _ShutDownSystemCommand;
        public ICommand ShutDownSystemCommand
        {
            get
            {
                if (null == _ShutDownSystemCommand) _ShutDownSystemCommand = new AsyncCommand(ShutDownSystem, false);
                return _ShutDownSystemCommand;
            }
        }

        private async Task ShutDownSystem()
        {
            try
            {

                EnumMessageDialogResult ret;

                //if (this.SequenceEngineManager().GetRunState(false) == true)
                if (this.SequenceEngineManager().GetMovingState() == true)
                {
                    ret = await this.MetroDialogManager().ShowMessageDialog("ShutDown", "Are you sure you want to turn off?", EnumMessageStyle.AffirmativeAndNegative);

                    if (ret == EnumMessageDialogResult.AFFIRMATIVE)
                    {
                        Window w = null;
                        IReleaseResource disposable = null;
                        System.Windows.Application.Current.Dispatcher.Invoke(() =>
                        {
                            w = System.Windows.Application.Current.MainWindow;
                            disposable = System.Windows.Application.Current.MainWindow as IReleaseResource;
                        });

                        disposable.Release();
                        //Extensions_IParam.ProgramShutDown = true;

                        //System.Windows.Application.Current.Dispatcher.Invoke(() =>
                        //{
                        //    w.Close();
                        //});
                    }
                }
                else
                {
                    ret = await this.MetroDialogManager().ShowMessageDialog("ShutDown", "Because the operation finished yet, can not turn off.", EnumMessageStyle.Affirmative);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

                
        private RelayCommand _EditResourceFileCommand;
        public ICommand EditResourceFileCommand
        {
            get
            {
                if (null == _EditResourceFileCommand) _EditResourceFileCommand = new RelayCommand(EditResourceFile);
                return _EditResourceFileCommand;
            }
        }
        private void EditResourceFile()
        {
            try
            {
                //UserControls.EditResource.EditResourceFile edit =
                //    new UserControls.EditResource.EditResourceFile(_ProbeView._MainWindows);
                //_ProbeView._MainWindows.OriginContent = _ProbeView._MainWindows.Content;
                //_ProbeView._MainWindows.Content = edit;
                //edit.DataContext = edit;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        //private RelayCommand<CUI.MenuItem> _LogOutCommand;
        //public ICommand LogOutCommand
        //{
        //    get
        //    {
        //        if (null == _LogOutCommand) _LogOutCommand = new RelayCommand<CUI.MenuItem>(LogOut);
        //        return _LogOutCommand;
        //    }
        //}
        //private void LogOut(CUI.MenuItem cuiparam)
        //{
        //    Guid ViewGUID = CUIService.GetTargetViewGUID(cuiparam.GUID);
        //    this.ViewModelManager().ViewTransition(ViewGUID);
        //}

        private RelayCommand _SetupThemeCommand;
        public ICommand SetupThemeCommand
        {
            get
            {
                if (null == _SetupThemeCommand) _SetupThemeCommand = new RelayCommand(SetupTheme);
                return _SetupThemeCommand;
            }
        }
        private void SetupTheme()
        {

        }


        //private RelayCommand<CUI.MenuItem> _ViewTransitionCommand;
        //public ICommand ViewTransitionCommand
        //{
        //    get
        //    {
        //        if (null == _ViewTransitionCommand) _ViewTransitionCommand = new RelayCommand<CUI.MenuItem>(ViewTransition);
        //        return _ViewTransitionCommand;
        //    }
        //}
        //private void ViewTransition(CUI.MenuItem cuiparam)
        //{
        //    Guid ViewGUID = CUIService.GetTargetViewGUID(cuiparam.GUID);
        //    this.ViewModelManager().ViewTransition(ViewGUID);
        //}
        #endregion


        private RelayCommand<object> _MenuBtnCommand_2;
        public ICommand MenuBtnCommand_2
        {
            get
            {
                if (null == _MenuBtnCommand_2) _MenuBtnCommand_2 = new RelayCommand<object>(MenuBtnFunc_2);
                return _MenuBtnCommand_2;
            }
        }

        private void MenuBtnFunc_2(object noparam)
        {

        }

        private RelayCommand _SnapShotCommand;
        public ICommand SnapShotCommand
        {
            get
            {
                if (null == _SnapShotCommand) _SnapShotCommand = new RelayCommand(FuncSnapShotCommand);
                return _SnapShotCommand;
            }
        }


        private void FuncSnapShotCommand()
        {
            try
            {
                string filepath;
                string filename;
                string fullpath;
                string fileextension;

                System.Drawing.Image screen = ScreenshotCapture.TakeScreenshot(true);

                filepath = @"C:\ProberSystem\Snapshot";
                filename = DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss");
                fileextension = ".jpg";

                filename = filename + fileextension;

                fullpath = Path.Combine(filepath, filename);

                if (Directory.Exists(Path.GetDirectoryName(fullpath)) == false)
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(fullpath));
                }

                if (File.Exists(fullpath) == false)
                {
                    screen.Save(fullpath);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
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

        #endregion
    }
}

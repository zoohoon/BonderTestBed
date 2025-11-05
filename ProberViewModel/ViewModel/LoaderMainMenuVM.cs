using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LoaderMainMenuViewModel
{
    using LoaderBase.Communication;
    using LogModule;
    using ProberErrorCode;
    using ProberInterfaces;
    using RelayCommandBase;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Windows;
    using System.Windows.Input;
    using Autofac;
    using ProberInterfaces.Loader;
    using MetroDialogInterfaces;
    using ProberInterfaces.ProberSystem;
    using UcDataForSettingPage;
    using ProberInterfaces.ViewModel;
    using LoaderBase;
    using VirtualKeyboardControl;
    using System.Diagnostics;
    using AccountModule;
    using SecsGemSettingDlg;

    public class LoaderMainMenuVM : IFactoryModule, INotifyPropertyChanged, ILoaderMainMenuVM, IMainScreenViewModel
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        public bool Initialized { get; set; } = false;

        readonly Guid _ViewModelGUID = new Guid("dd198654-f594-4a04-939a-5cc8e678a873");
        public Guid ScreenGUID { get { return _ViewModelGUID; } }

        #region //..Property

        private Visibility _MenuOpenVisibility = Visibility.Visible;
        public Visibility MenuOpenVisibility
        {
            get { return _MenuOpenVisibility; }
            set
            {
                if (value != _MenuOpenVisibility)
                {
                    _MenuOpenVisibility = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Visibility _MenuCloseVisibility = Visibility.Hidden;
        public Visibility MenuCloseVisibility
        {
            get { return _MenuCloseVisibility; }
            set
            {
                if (value != _MenuCloseVisibility)
                {
                    _MenuCloseVisibility = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _isExpanded_Communication = false;
        public bool isExpanded_Communication
        {
            get { return _isExpanded_Communication; }
            set
            {
                if (value != _isExpanded_Communication)
                {
                    _isExpanded_Communication = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _isExpanded_Recipe = false;
        public bool isExpanded_Recipe
        {
            get { return _isExpanded_Recipe; }
            set
            {
                if (value != _isExpanded_Recipe)
                {
                    _isExpanded_Recipe = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _isExpanded_System = false;
        public bool isExpanded_System
        {
            get { return _isExpanded_System; }
            set
            {
                if (value != _isExpanded_System)
                {
                    _isExpanded_System = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _isExpanded_Loader = false;
        public bool isExpanded_Loader
        {
            get { return _isExpanded_Loader; }
            set
            {
                if (value != _isExpanded_Loader)
                {
                    _isExpanded_Loader = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _isExpanded_Operation = false;
        public bool isExpanded_Operation
        {
            get { return _isExpanded_Operation; }
            set
            {
                if (value != _isExpanded_Operation)
                {
                    _isExpanded_Operation = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _isExpanded_Utility = false;
        public bool isExpanded_Utility
        {
            get { return _isExpanded_Utility; }
            set
            {
                if (value != _isExpanded_Utility)
                {
                    _isExpanded_Utility = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _isExpanded_Setting = false;
        public bool isExpanded_Setting
        {
            get { return _isExpanded_Setting; }
            set
            {
                if (value != _isExpanded_Setting)
                {
                    _isExpanded_Setting = value;
                    RaisePropertyChanged();
                }
            }
        }


        private bool _isExpanded_Power = false;
        public bool isExpanded_Power
        {
            get { return _isExpanded_Power; }
            set
            {
                if (value != _isExpanded_Power)
                {
                    _isExpanded_Power = value;
                    RaisePropertyChanged();
                }
            }
        }
        private Guid CUIGuid;
        private Guid ChuckPlanarityGuid;
        private Guid SetupWithCellGuid;
        private Guid ManualContactGuid;
        private Guid MultiManualContactGuid;
        private Guid WaferInspectionGuid;
        private Guid ManualPolishWaferGuid;
        private Guid ManualSoakingGuid;

        private Guid CurCellPageGuid;
        private Guid ZeroNumsGuid;
        private Guid AccountViewGuid;

        private Guid PMIViewerGuid;
        private Guid LogCollectControlGuid;
        private Guid FoupRecoveryViewGuid;


        private Guid HandlingViewGuid;

        private List<Guid> CardUpModuleCheckGuidList;
        private List<Guid> CardUpModuleAndVacuumCheckGuidList;
        private List<Guid> AllowableOnlineModelist;

        public ILoaderSupervisor LoaderMaster => this.GetLoaderContainer().Resolve<ILoaderSupervisor>();

        #endregion
        public LoaderMainMenuVM()
        {
            loaderCommunicationManager = this.GetLoaderContainer().Resolve<ILoaderCommunicationManager>();

            CUIGuid = new Guid("B0737A90-696C-9155-D4C9-A628D1EED129");
            ChuckPlanarityGuid = new Guid("d3b97c85-2bee-4fd8-834f-bb4c3401752a");
            SetupWithCellGuid = new Guid("5FBF9EB1-C022-4FE6-A665-6569D45EFD3A");
            ManualContactGuid = new Guid("ac247488-2cb3-4250-9cfd-bd6852802a83");
            MultiManualContactGuid = new Guid("6c307199-ddcd-496d-89f7-a462bd13f949");
            WaferInspectionGuid = new Guid("f8396e3a-b8ce-4dcd-9a0d-643532a7d9d1");
            ManualPolishWaferGuid = new Guid("E54FE805-133A-A76E-1EEE-E6D4DA81FFC9");
            ManualSoakingGuid = new Guid("8B2993EA-7358-43CD-91BC-BAD430C0A9F4");
            AccountViewGuid = new Guid("67a5e6b4-c986-4323-b05c-9139e703a0f9");
            PMIViewerGuid = new Guid("90cc9901-72d7-451e-94a4-daf3aa6931ea");
            LogCollectControlGuid = new Guid("da1adafa-0d52-401b-937a-6b1ba5cae886");

            FoupRecoveryViewGuid = new Guid("F7BE0142-1ED7-4483-A257-AC512454F6F2");
            switch (SystemManager.SystemType)
            {
                case SystemTypeEnum.None:
                    break;
                case SystemTypeEnum.Opera:
                    HandlingViewGuid = new Guid("156F45C2-472E-A15D-1B1E-793F7E22DCA4");
                    break;
                case SystemTypeEnum.GOP:
                    HandlingViewGuid = new Guid("da72dfc3-4a34-4206-b321-4bdbf074de7d");
                    break;
                case SystemTypeEnum.DRAX:
                    HandlingViewGuid = new Guid("758ceac2-5962-4810-a8ab-7719d2b12f0c");
                    break;
                default:
                    break;
            }

            CurCellPageGuid = new Guid();
            ZeroNumsGuid = new Guid();
            CardUpModuleCheckGuidList = new List<Guid>();
            CardUpModuleCheckGuidList.Add(new Guid("B0737A90-696C-9155-D4C9-A628D1EED129")); //cui
            CardUpModuleCheckGuidList.Add(new Guid("d3b97c85-2bee-4fd8-834f-bb4c3401752a")); //ChuckPlanarity
            CardUpModuleCheckGuidList.Add(new Guid("5FBF9EB1-C022-4FE6-A665-6569D45EFD3A")); //SetupWithCell
            CardUpModuleCheckGuidList.Add(new Guid("ac247488-2cb3-4250-9cfd-bd6852802a83")); //ManualContact
            CardUpModuleCheckGuidList.Add(new Guid("f8396e3a-b8ce-4dcd-9a0d-643532a7d9d1")); //waferInspection
            CardUpModuleCheckGuidList.Add(new Guid("E54FE805-133A-A76E-1EEE-E6D4DA81FFC9")); //ManualPolishWafer
            //CardUpModuleCheckGuidList.Add(new Guid("8B2993EA-7358-43CD-91BC-BAD430C0A9F4")); //Manual Soaking

            CardUpModuleAndVacuumCheckGuidList = new List<Guid>();
            CardUpModuleAndVacuumCheckGuidList.Add(new Guid("6c647706-8060-447a-992e-6c8893b0f025")); //CardChangeOPPage - DRAX
            CardUpModuleAndVacuumCheckGuidList.Add(new Guid("4b9c1445-bf91-4118-b572-0106a03f2524")); //CardChangeOPPage


            AllowableOnlineModelist = new List<Guid>();
            AllowableOnlineModelist.Add(PMIViewerGuid);
            AllowableOnlineModelist.Add(LogCollectControlGuid);
        }
        private ILoaderCommunicationManager loaderCommunicationManager;

        #region //..Command

        private RelayCommand _MenuOpenclickCommand;
        public ICommand MenuOpenclickCommand
        {
            get
            {
                if (null == _MenuOpenclickCommand) _MenuOpenclickCommand = new RelayCommand(FuncMenuOpenclickCommand);
                return _MenuOpenclickCommand;
            }
        }
        private void FuncMenuOpenclickCommand()
        {
            try
            {
                MenuOpenVisibility = Visibility.Collapsed;
                MenuCloseVisibility = Visibility.Visible;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand _MenuCloseclickCommand;
        public ICommand MenuCloseclickCommand
        {
            get
            {
                if (null == _MenuCloseclickCommand) _MenuCloseclickCommand = new RelayCommand(FuncMenuCloseclickCommand);
                return _MenuCloseclickCommand;
            }
        }
        //public void CloseMenu()
        //{
        //    //try
        //    //{

        //    //    FuncMenuCloseclickCommand();
        //    //}
        //    //catch (Exception err)
        //    //{
        //    //    LoggerManager.Exception(err);
        //    //}
        //}

        private void FuncMenuCloseclickCommand()
        {
            try
            {
                this.ViewModelManager().ChangeFlyOutControlStatus(false);

                MenuOpenVisibility = Visibility.Visible;
                MenuCloseVisibility = Visibility.Collapsed;
                isExpanded_Communication = false;
                isExpanded_Operation = false;
                isExpanded_Utility = false;
                isExpanded_Setting = false;
                isExpanded_Power = false;
                isExpanded_Recipe = false;
                isExpanded_Loader = false;
                isExpanded_System = false;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        #region //.. Menu

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
                //Guid ViewGUID = CUIService.GetTargetViewGUID(cuiparam.GUID);
                Guid ViewGUID = new Guid();
                await this.ViewModelManager().ViewTransitionAsync(ViewGUID);
                FuncMenuCloseclickCommand();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }



        private RelayCommand _CommunicationMenuOpenclickCommand;
        public ICommand CommunicationMenuOpenclickCommand
        {
            get
            {
                if (null == _CommunicationMenuOpenclickCommand) _CommunicationMenuOpenclickCommand = new RelayCommand(CommunicationMenuOpenclickCommandFunc);
                return _CommunicationMenuOpenclickCommand;
            }
        }
        private void CommunicationMenuOpenclickCommandFunc()
        {
            try
            {

                //MenuOpenVisibility = Visibility.Collapsed;
                //MenuCloseVisibility = Visibility.Visible;

                isExpanded_Communication = !isExpanded_Communication;
                isExpanded_Setting = false;
                isExpanded_Power = false;
                isExpanded_Operation = false;
                isExpanded_Utility = false;
                isExpanded_Recipe = false;
                isExpanded_System = false;
                isExpanded_Loader = false;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        private RelayCommand _OperationMenuOpenclickCommand;
        public ICommand OperationMenuOpenclickCommand
        {
            get
            {
                if (null == _OperationMenuOpenclickCommand) _OperationMenuOpenclickCommand = new RelayCommand(OperationMenuOpenclickCommandFunc);
                return _OperationMenuOpenclickCommand;
            }
        }
        private void OperationMenuOpenclickCommandFunc()
        {
            try
            {

                MenuOpenVisibility = Visibility.Collapsed;

                MenuCloseVisibility = Visibility.Visible;
                isExpanded_Communication = false;
                isExpanded_Setting = false;
                isExpanded_Operation = !isExpanded_Operation;
                isExpanded_Utility = false;
                isExpanded_Recipe = false;
                isExpanded_System = false;
                isExpanded_Loader = false;
                isExpanded_Power = false;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand _UtilityMenuOpenclickCommand;
        public ICommand UtilityMenuOpenclickCommand
        {
            get
            {
                if (null == _UtilityMenuOpenclickCommand) _UtilityMenuOpenclickCommand = new RelayCommand(UtilityMenuOpenclickCommandFunc);
                return _UtilityMenuOpenclickCommand;
            }
        }
        private void UtilityMenuOpenclickCommandFunc()
        {
            try
            {

                MenuOpenVisibility = Visibility.Collapsed;
                MenuCloseVisibility = Visibility.Visible;

                isExpanded_Communication = false;
                isExpanded_Setting = false;
                isExpanded_Operation = false;
                isExpanded_Utility = !isExpanded_Utility;
                isExpanded_Recipe = false;
                isExpanded_System = false;
                isExpanded_Loader = false;
                isExpanded_Power = false;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand _SettingMenuOpenclickCommand;
        public ICommand SettingMenuOpenclickCommand
        {
            get
            {
                if (null == _SettingMenuOpenclickCommand) _SettingMenuOpenclickCommand = new RelayCommand(SettingMenuOpenclickCommandFunc);
                return _SettingMenuOpenclickCommand;
            }
        }
        private void SettingMenuOpenclickCommandFunc()
        {
            try
            {
                MenuOpenVisibility = Visibility.Collapsed;
                MenuCloseVisibility = Visibility.Visible;

                isExpanded_Communication = false;
                isExpanded_Setting = !isExpanded_Setting;
                isExpanded_Operation = false;
                isExpanded_Utility = false;
                isExpanded_Recipe = false;
                isExpanded_System = false;
                isExpanded_Loader = false;
                isExpanded_Power = false;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        private RelayCommand _RecipeMenuOpenclickCommand;
        public ICommand RecipeMenuOpenclickCommand
        {
            get
            {
                if (null == _RecipeMenuOpenclickCommand) _RecipeMenuOpenclickCommand = new RelayCommand(RecipeMenuOpenclickCommandFunc);
                return _RecipeMenuOpenclickCommand;
            }
        }
        private void RecipeMenuOpenclickCommandFunc()
        {
            try
            {

                MenuOpenVisibility = Visibility.Collapsed;
                MenuCloseVisibility = Visibility.Visible;

                isExpanded_Communication = false;
                isExpanded_Setting = false;
                isExpanded_Operation = false;
                isExpanded_Utility = false;
                isExpanded_Recipe = !isExpanded_Recipe;
                isExpanded_System = false;
                isExpanded_Power = false;
                isExpanded_Loader = false;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand _SystemMenuOpenclickCommand;
        public ICommand SystemMenuOpenclickCommand
        {
            get
            {
                if (null == _SystemMenuOpenclickCommand) _SystemMenuOpenclickCommand = new RelayCommand(SystemMenuOpenclickCommandFunc);
                return _SystemMenuOpenclickCommand;
            }
        }
        private void SystemMenuOpenclickCommandFunc()
        {
            try
            {

                MenuOpenVisibility = Visibility.Collapsed;
                MenuCloseVisibility = Visibility.Visible;

                isExpanded_Communication = false;
                isExpanded_Setting = false;
                isExpanded_Operation = false;
                isExpanded_Utility = false;
                isExpanded_Recipe = false;
                isExpanded_Power = false;
                isExpanded_System = !isExpanded_System;
                isExpanded_Loader = false;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand _LoaderMenuOpenclickCommand;
        public ICommand LoaderMenuOpenclickCommand
        {
            get
            {
                if (null == _LoaderMenuOpenclickCommand) _LoaderMenuOpenclickCommand = new RelayCommand(LoaderMenuOpenclickCommandFunc);
                return _LoaderMenuOpenclickCommand;
            }
        }
        private void LoaderMenuOpenclickCommandFunc()
        {
            try
            {
                MenuOpenVisibility = Visibility.Collapsed;
                MenuCloseVisibility = Visibility.Visible;

                isExpanded_Communication = false;
                isExpanded_Setting = false;
                isExpanded_Operation = false;
                isExpanded_Utility = false;
                isExpanded_Recipe = false;
                isExpanded_System = false;
                isExpanded_Loader = !isExpanded_Loader;
                isExpanded_Power = false;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand _PowerMenuOpenclickCommand;
        public ICommand PowerMenuOpenclickCommand
        {
            get
            {
                if (null == _PowerMenuOpenclickCommand)
                    _PowerMenuOpenclickCommand = new RelayCommand(PowerMenuOpenclickCommandFunc);
                return _PowerMenuOpenclickCommand;
            }
        }
        private void PowerMenuOpenclickCommandFunc()
        {
            try
            {

                MenuOpenVisibility = Visibility.Collapsed;
                MenuCloseVisibility = Visibility.Visible;

                isExpanded_Communication = false;
                isExpanded_Setting = false;
                isExpanded_Power = !isExpanded_Power;
                isExpanded_Operation = false;
                isExpanded_Utility = false;
                isExpanded_Recipe = false;
                isExpanded_System = false;
                isExpanded_Loader = false;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        private AsyncCommand _LogOutClickCommand;
        public ICommand LogOutClickCommand
        {
            get
            {
                if (null == _LogOutClickCommand)
                    _LogOutClickCommand = new AsyncCommand(LogOutClickCommandFunc);
                return _LogOutClickCommand;
            }
        }
        private async Task LogOutClickCommandFunc()
        {
            try
            {
                if (!IsLoaderBusy())
                {
                    EnumMessageDialogResult ret;
                    ret = await this.MetroDialogManager().ShowMessageDialog("Logout", "Are you sure you want to logout?", EnumMessageStyle.AffirmativeAndNegative);

                    if (ret == EnumMessageDialogResult.AFFIRMATIVE)
                    {
                        LoggerManager.Debug($"[LoaderMainMenuVM] Logout");
                        this.ViewModelManager().ViewTransitionAsync(new Guid("28A11F12-8918-47FE-8161-3652F2EFEF29"));
                    }
                }
                else
                {
                    await this.MetroDialogManager().ShowMessageDialog("[Information]", $"Loader State is {LoaderMaster.ModuleState.State.ToString()} State. Please Check the Loader State.", EnumMessageStyle.Affirmative);
                }

                FuncMenuCloseclickCommand();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        private AsyncCommand _ProbeCardSetupPageSwitchingCommand;
        public ICommand ProbeCardSetupPageSwitchingCommand
        {
            get
            {
                if (null == _ProbeCardSetupPageSwitchingCommand) _ProbeCardSetupPageSwitchingCommand = new AsyncCommand(FuncProbeCardSetupPageSwitchingCommand);
                return _ProbeCardSetupPageSwitchingCommand;
            }
        }
        private async Task FuncProbeCardSetupPageSwitchingCommand()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (DeviceCategories.Count < 1)
                {
                    InitDeviceCategories();
                }
                CurCellPageGuid = CUIGuid;
                retval = await CanChangeScreenForMaintenance();

                if (retval == EventCodeEnum.NONE)
                {
                    retval = await CanChangeScreenForManualSoaking();
                    if (retval == EventCodeEnum.NONE)
                    {
                        var category = DeviceCategories.Where(c => c.Name == "Probe Card");
                        Category cat = category.FirstOrDefault();
                        if (cat != null)
                        {
                            //await this.ViewModelManager().ViewTransitionAsync(new Guid("B0737A90-696C-9155-D4C9-A628D1EED129"));
                            await this.ViewModelManager().ViewTransitionAsync(new Guid("B0737A90-696C-9155-D4C9-A628D1EED129"), cat);
                        }
                    }
                }
                FuncMenuCloseclickCommand();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                CurCellPageGuid = ZeroNumsGuid;
                //await this.WaitCancelDialogService().CloseDialog();
            }
        }

        private AsyncCommand _WaferSetupPageSwitchingCommand;
        public ICommand WaferSetupPageSwitchingCommand
        {
            get
            {
                if (null == _WaferSetupPageSwitchingCommand) _WaferSetupPageSwitchingCommand = new AsyncCommand(FuncWaferSetupPageSwitchingCommand);
                return _WaferSetupPageSwitchingCommand;
            }
        }
        private async Task FuncWaferSetupPageSwitchingCommand()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (DeviceCategories.Count < 1)
                {
                    InitDeviceCategories();
                }
                CurCellPageGuid = CUIGuid;
                retval = await CanChangeScreenForMaintenance();

                if (retval == EventCodeEnum.NONE)
                {
                    retval = await CanChangeScreenForManualSoaking();
                    if (retval == EventCodeEnum.NONE)
                    {
                        var category = DeviceCategories.Where(c => c.Name == "Wafer");
                        Category cat = category.FirstOrDefault();
                        if (cat != null)
                        {
                            await this.ViewModelManager().ViewTransitionAsync(new Guid("B0737A90-696C-9155-D4C9-A628D1EED129"), cat);
                        }
                    }
                }
                FuncMenuCloseclickCommand();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                CurCellPageGuid = ZeroNumsGuid;
            }
        }

        private AsyncCommand _PMISetupPageSwitchingCommand;
        public ICommand PMISetupPageSwitchingCommand
        {
            get
            {
                if (null == _PMISetupPageSwitchingCommand) _PMISetupPageSwitchingCommand = new AsyncCommand(FuncPMISetupPageSwitchingCommand);
                return _PMISetupPageSwitchingCommand;
            }
        }
        private async Task FuncPMISetupPageSwitchingCommand()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (DeviceCategories.Count < 1)
                {
                    InitDeviceCategories();
                }
                CurCellPageGuid = CUIGuid;

                retval = await CanChangeScreenForMaintenance();
                if (retval == EventCodeEnum.NONE)
                {
                    retval = await CanChangeScreenForManualSoaking();
                    if (retval == EventCodeEnum.NONE)
                    {
                        var category = DeviceCategories.Where(c => c.Name == "Probe Mark");
                        Category cat = category.FirstOrDefault();
                        if (cat != null)
                        {
                            await this.ViewModelManager().ViewTransitionAsync(new Guid("B0737A90-696C-9155-D4C9-A628D1EED129"), cat);
                        }
                    }
                }
                FuncMenuCloseclickCommand();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                CurCellPageGuid = ZeroNumsGuid;
                //await this.WaitCancelDialogService().CloseDialog();
            }
        }

        private AsyncCommand _OCRSetupPageSwitchingCommand;
        public ICommand OCRSetupPageSwitchingCommand
        {
            get
            {
                if (null == _OCRSetupPageSwitchingCommand) _OCRSetupPageSwitchingCommand = new AsyncCommand(FuncOCRSetupPageSwitchingCommand);
                return _OCRSetupPageSwitchingCommand;
            }
        }
        private async Task FuncOCRSetupPageSwitchingCommand()
        {
            try
            {
                if (!IsLoaderBusy())
                {
                    await this.ViewModelManager().ViewTransitionAsync(new Guid("42D9D35A-D5E6-4799-B0AE-03F9900B52C3"));
                }
                else
                {
                    await this.MetroDialogManager().ShowMessageDialog("[Information]", $"Loader State is {LoaderMaster.ModuleState.State.ToString()} State. Please Check the Loader State.", EnumMessageStyle.Affirmative);
                }

                FuncMenuCloseclickCommand();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private AsyncCommand _DeviceChangePageSwitchingCommand;
        public ICommand DeviceChangePageSwitchingCommand
        {
            get
            {
                if (null == _DeviceChangePageSwitchingCommand) _DeviceChangePageSwitchingCommand = new AsyncCommand(FuncDeviceChangePageSwitchingCommand);
                return _DeviceChangePageSwitchingCommand;
            }
        }
        private async Task FuncDeviceChangePageSwitchingCommand()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                retval = await CanChangeScreenForMaintenance();
                if (retval == EventCodeEnum.NONE)
                {
                    retval = await CanChangeScreenForManualSoaking();
                    if (retval == EventCodeEnum.NONE)
                    {
                        await this.ViewModelManager().ViewTransitionAsync(new Guid("956bb44f-4b89-42b3-b21a-69f896a840fe"));
                    }
                }

                FuncMenuCloseclickCommand();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private AsyncCommand _TempCalPageSwitchingCommand;
        public ICommand TempCalPageSwitchingCommand
        {
            get
            {
                if (null == _TempCalPageSwitchingCommand) _TempCalPageSwitchingCommand = new AsyncCommand(FuncTempCalPageSwitchingCommand);
                return _TempCalPageSwitchingCommand;
            }
        }
        private async Task FuncTempCalPageSwitchingCommand()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                retval = await CanChangeScreenForMaintenance();
                if (retval == EventCodeEnum.NONE)
                {
                    retval = await CanChangeScreenForManualSoaking();
                    if (retval == EventCodeEnum.NONE)
                    {
                        await this.ViewModelManager().ViewTransitionAsync(new Guid("e905653f-52ff-460a-b7bf-d82f8996c0d8"));
                    }
                }

                FuncMenuCloseclickCommand();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        //CardChangeOPPageSwitchingCommand
        private AsyncCommand _CardChangeOPPageSwitchingCommand;
        public ICommand CardChangeOPPageSwitchingCommand
        {
            get
            {
                if (null == _CardChangeOPPageSwitchingCommand) _CardChangeOPPageSwitchingCommand = new AsyncCommand(CardChangeOPPageSwitchingCommandFunc);
                return _CardChangeOPPageSwitchingCommand;
            }
        }
        private async Task CardChangeOPPageSwitchingCommandFunc()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                retval = await CanChangeScreenForMaintenance();

                if (retval == EventCodeEnum.NONE)
                {
                    retval = await CanChangeScreenForManualSoaking();
                    if (retval == EventCodeEnum.NONE)
                    {
                        retval = await CanChangeScreenForStageLock();
                        if(retval == EventCodeEnum.NONE)
                        {
                            if (SystemManager.SystemType == SystemTypeEnum.DRAX)
                            {
                                await this.ViewModelManager().ViewTransitionAsync(new Guid("6c647706-8060-447a-992e-6c8893b0f025"));
                            }
                            else if (!IsLoaderBusy())
                            {
                                await this.ViewModelManager().ViewTransitionAsync(new Guid("4b9c1445-bf91-4118-b572-0106a03f2524"));
                            }
                            else
                            {
                                await this.MetroDialogManager().ShowMessageDialog(
                                    "[Information]", $"Loader State is {LoaderMaster.ModuleState.State.ToString()} State. Please Check the Loader State.", EnumMessageStyle.Affirmative);
                            }
                        }
                    }
                }

                FuncMenuCloseclickCommand();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        private AsyncCommand _CardChangeObservationOPPageSwitchingCommand;
        public ICommand CardChangeObservationOPPageSwitchingCommand
        {
            get
            {
                if (null == _CardChangeObservationOPPageSwitchingCommand) _CardChangeObservationOPPageSwitchingCommand = new AsyncCommand(CardChangeObservationOPPageSwitchingCommandFunc);
                return _CardChangeObservationOPPageSwitchingCommand;
            }
        }
        private async Task CardChangeObservationOPPageSwitchingCommandFunc()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                var remoteProxy = loaderCommunicationManager.GetProxy<IRemoteMediumProxy>();
                if (remoteProxy != null)
                {
                    var isCardPodModuleCheckState = remoteProxy.IsCheckCardPodState();
                    if (isCardPodModuleCheckState == false)
                    {
                        await this.MetroDialogManager().ShowMessageDialog("Can not switch page", "Card pod up module is up and the vacuum is not detected.", MetroDialogInterfaces.EnumMessageStyle.Affirmative);
                        //return EventCodeEnum.GP_CardChange_CHECK_TO_CARD_UP_MOUDLE;
                    }
                    else
                    {
                        retval = await CanChangeScreenForMaintenance();

                        if (retval == EventCodeEnum.NONE)
                        {
                            retval = await CanChangeScreenForManualSoaking();
                            if (retval == EventCodeEnum.NONE)
                            {
                                retval = await CanChangeScreenForStageLock();
                                if (retval == EventCodeEnum.NONE)
                                {
                                    CardChangeVacuumAndIOStatus data = null;

                                    Task task = new Task(() =>
                                    {
                                        var remoteproxy = loaderCommunicationManager.GetProxy<IRemoteMediumProxy>();
                                        data = remoteproxy.GPCC_OP_GetCCVacuumStatus();
                                    });
                                    task.Start();
                                    await task;

                                    //await Task.Run(() => {
                                    //    var remoteproxy = loaderCommunicationManager.GetRemoteMediumClient();
                                    //    data = remoteproxy.GPCC_OP_GetCCVacuumStatus();
                                    //});

                                    if (data?.IsCardLatchLock == false &&
                                        data?.IsCardLatchUnLock == true &&
                                        data?.CardPogoTouched == false
                                        || Extensions_IParam.ProberRunMode == RunMode.EMUL)
                                    {
                                        if (SystemManager.SystemType == SystemTypeEnum.DRAX)
                                        {
                                            await this.ViewModelManager().ViewTransitionAsync(new Guid("b094fbf9-35a0-43ab-9311-def5a717a9f7"));
                                        }
                                        else
                                        {
                                            await this.ViewModelManager().ViewTransitionAsync(new Guid("b7104207-1f96-4669-b027-03061794d5a5"));
                                        }
                                    }
                                    else
                                    {
                                        this.MetroDialogManager().ShowMessageDialog("Can not switch page", "have to check that card latch and probe card docking status",
                                            EnumMessageStyle.Affirmative);
                                    }
                                }
                            }
                        }
                    }
                }

                FuncMenuCloseclickCommand();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        private AsyncCommand _TesterInterfacePageSwitchingCommand;
        public ICommand TesterInterfacePageSwitchingCommand
        {
            get
            {
                if (null == _TesterInterfacePageSwitchingCommand) _TesterInterfacePageSwitchingCommand = new AsyncCommand(TesterInterfacePageSwitchingCommandFunc);
                return _TesterInterfacePageSwitchingCommand;
            }
        }
        private async Task TesterInterfacePageSwitchingCommandFunc()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                retval = await CanChangeScreenForMaintenance();
                if (retval == EventCodeEnum.NONE)
                {
                    retval = await CanChangeScreenForManualSoaking();
                    if (retval == EventCodeEnum.NONE)
                    {
                        retval = await CanChangeScreenForStageLock();
                        if (retval == EventCodeEnum.NONE)
                        {
                            if (SystemManager.SystemType == SystemTypeEnum.DRAX)
                            {
                                await this.ViewModelManager().ViewTransitionAsync(new Guid("62567D35-0DA9-4A65-884C-079A4A693916"));
                            }
                            else
                            {
                            }
                        }
                    }
                }

                FuncMenuCloseclickCommand();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private AsyncCommand _PolishWaferManualPageSwitchingCommand;
        public ICommand PolishWaferManualPageSwitchingCommand
        {
            get
            {
                if (null == _PolishWaferManualPageSwitchingCommand) _PolishWaferManualPageSwitchingCommand = new AsyncCommand(PolishWaferManualPageSwitchingCommandFunc);
                return _PolishWaferManualPageSwitchingCommand;
            }
        }
        private async Task PolishWaferManualPageSwitchingCommandFunc()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                CurCellPageGuid = ManualPolishWaferGuid;

                    retval = await CanChangeScreenForMaintenance();

                if (retval == EventCodeEnum.NONE)
                {
                    retval = await CanChangeScreenForManualSoaking();
                    if (retval == EventCodeEnum.NONE)
                    {
                        retval = await CanChangeScreenForStageLock();
                        if (retval == EventCodeEnum.NONE)
                        {
                            if (!IsLoaderBusy())
                            {
                                IStageSupervisorProxy stageproxy = null;

                                stageproxy = loaderCommunicationManager.GetProxy<IStageSupervisorProxy>();

                                EnumSubsStatus waferstatus = stageproxy.GetWaferStatus();
                                EnumWaferType wafertype = stageproxy.GetWaferType();

                                if (waferstatus == EnumSubsStatus.EXIST &&
                                    wafertype != EnumWaferType.POLISH)
                                {
                                    await this.MetroDialogManager().ShowMessageDialog("[Information]"
                                        , $"The cell loaded a wafer of the wrong wafer type." +
                                        $"\nThe loaded wafer type must be \"POLISH\" or the Wafer status of the cell must be \"NOT EXIST\" to enter the screen."
                                        , EnumMessageStyle.Affirmative);
                                    LoggerManager.Debug($"PolishWaferManualPageSwitchingCommandFunc(), The cell loaded a wafer of the wrong wafer type." +
                                        $"Wafer Status: {waferstatus}, Wafer Type: {wafertype}");
                                }
                                else
                                {
                                    await this.ViewModelManager().ViewTransitionAsync(new Guid("E54FE805-133A-A76E-1EEE-E6D4DA81FFC9"));
                                }
                            }
                            else
                            {
                                await this.MetroDialogManager().ShowMessageDialog(
                                    "[Information]", $"Loader State is {LoaderMaster.ModuleState.State.ToString()} State. Please Check the Loader State.", EnumMessageStyle.Affirmative);
                            }
                        }
                    }
                }

                FuncMenuCloseclickCommand();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                CurCellPageGuid = ZeroNumsGuid;
            }
        }

        // [BEGIN]
        private AsyncCommand _ManualSoakingPageSwitchingCommand;
        public ICommand ManualSoakingPageSwitchingCommand
        {
            get
            {
                if (null == _ManualSoakingPageSwitchingCommand) _ManualSoakingPageSwitchingCommand = new AsyncCommand(ManualSoakingPageSwitchingCommandFunc);
                return _ManualSoakingPageSwitchingCommand;
            }
        }
        private async Task ManualSoakingPageSwitchingCommandFunc()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                CurCellPageGuid = ManualSoakingGuid;
                retval = await CanChangeScreenForMaintenance();
                if (retval == EventCodeEnum.NONE)
                {
                    retval = await CanChangeScreenForStageLock();
                    if (retval == EventCodeEnum.NONE)
                    {
                        await this.ViewModelManager().ViewTransitionAsync(new Guid("8B2993EA-7358-43CD-91BC-BAD430C0A9F4"));
                    }
                }

                FuncMenuCloseclickCommand();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        // [END]

        private AsyncCommand _TouchSensorSetupPageSwitchingCommand;
        public ICommand TouchSensorSetupPageSwitchingCommand
        {
            get
            {
                if (null == _TouchSensorSetupPageSwitchingCommand) _TouchSensorSetupPageSwitchingCommand = new AsyncCommand(TouchSensorSetupPageSwitchingCommandFunc);
                return _TouchSensorSetupPageSwitchingCommand;
            }
        }
        private async Task TouchSensorSetupPageSwitchingCommandFunc()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (SystemCategories.Count < 1)
                {
                    InitSystemCategories();
                }

                retval = await CanChangeScreenForMaintenance();

                if (retval == EventCodeEnum.NONE)
                {
                    retval = await CanChangeScreenForManualSoaking();
                    if (retval == EventCodeEnum.NONE)
                    {
                        var category = SystemCategories.Where(c => c.Name == "Devices");
                        Category cat = category.FirstOrDefault();
                        if (cat != null)
                        {
                            var viewinfo = cat.SettingViewInfos.Where(c => c.Name == "Touch Sensor Setup");
                            SettingInfo view = viewinfo.FirstOrDefault();
                            if (view != null)
                            {
                                view.Visibility = Visibility.Visible;
                                await this.ViewModelManager().ViewTransitionAsync(new Guid("B0737A90-696C-9155-D4C9-A628D1EED129"), cat);
                            }
                        }
                    }
                }

                FuncMenuCloseclickCommand();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private AsyncCommand _AccountPageSwitchingCommand;
        public ICommand AccountPageSwitchingCommand
        {
            get
            {
                if (null == _AccountPageSwitchingCommand) _AccountPageSwitchingCommand = new AsyncCommand(AccountPageSwitchingCommandFunc);
                return _AccountPageSwitchingCommand;
            }
        }
        private async Task AccountPageSwitchingCommandFunc()
        {
            try
            {
                await this.ViewModelManager().ViewTransitionAsync(AccountViewGuid);
                FuncMenuCloseclickCommand();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private AsyncCommand _UtilityPMIViewerPageSwitchingCommand;
        public ICommand UtilityPMIViewerPageSwitchingCommand
        {
            get
            {
                if (null == _UtilityPMIViewerPageSwitchingCommand) _UtilityPMIViewerPageSwitchingCommand = new AsyncCommand(UtilityPMIViewerPageSwitchingCommandFunc);
                return _UtilityPMIViewerPageSwitchingCommand;
            }
        }
        private async Task UtilityPMIViewerPageSwitchingCommandFunc()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                CurCellPageGuid = PMIViewerGuid;
                retval = await CanChangeScreenForMaintenance();

                if (retval == EventCodeEnum.NONE)
                {
                    await this.ViewModelManager().ViewTransitionAsync(CurCellPageGuid);
                }

                FuncMenuCloseclickCommand();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                CurCellPageGuid = ZeroNumsGuid;
            }
        }


        private AsyncCommand _UtilityLogCollectControlPageSwitchingCommand;
        public ICommand UtilityLogCollectControlPageSwitchingCommand
        {
            get
            {
                if (null == _UtilityLogCollectControlPageSwitchingCommand) _UtilityLogCollectControlPageSwitchingCommand = new AsyncCommand(UtilityLogCollectControlPageSwitchingCommandFunc);
                return _UtilityLogCollectControlPageSwitchingCommand;
            }
        }
        private async Task UtilityLogCollectControlPageSwitchingCommandFunc()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                CurCellPageGuid = LogCollectControlGuid;
                await this.ViewModelManager().ViewTransitionAsync(CurCellPageGuid);

                FuncMenuCloseclickCommand();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                CurCellPageGuid = ZeroNumsGuid;
            }
        }

        private AsyncCommand _ChuckPlnarityPageSwitchingCommand;
        public ICommand ChuckPlnarityPageSwitchingCommand
        {
            get
            {
                if (null == _ChuckPlnarityPageSwitchingCommand) _ChuckPlnarityPageSwitchingCommand = new AsyncCommand(ChuckPlnarityPageSwitchingCommandFunc);
                return _ChuckPlnarityPageSwitchingCommand;
            }
        }
        private async Task ChuckPlnarityPageSwitchingCommandFunc()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                CurCellPageGuid = ChuckPlanarityGuid;
                retval = await CanChangeScreenForMaintenance();

                if (retval == EventCodeEnum.NONE)
                {
                    retval = await CanChangeScreenForManualSoaking();
                    if (retval == EventCodeEnum.NONE)
                    {
                        await this.ViewModelManager().ViewTransitionAsync(new Guid("d3b97c85-2bee-4fd8-834f-bb4c3401752a"));
                    }
                }
                FuncMenuCloseclickCommand();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                CurCellPageGuid = ZeroNumsGuid;
            }
        }

        private AsyncCommand _SysParamPageSwitchingCommand;
        public ICommand SysParamPageSwitchingCommand
        {
            get
            {
                if (null == _SysParamPageSwitchingCommand) _SysParamPageSwitchingCommand = new AsyncCommand(FuncSysParamPageSwitchingCommand);
                return _SysParamPageSwitchingCommand;
            }
        }
        private async Task FuncSysParamPageSwitchingCommand()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (SystemCategories.Count < 1)
                {
                    InitSystemCategories();
                }

                retval = await CanChangeScreenForMaintenance();

                if (retval == EventCodeEnum.NONE)
                {
                    retval = await CanChangeScreenForManualSoaking();
                    if (retval == EventCodeEnum.NONE)
                    {
                        var category = SystemCategories.Where(c => c.Name == "System");
                        Category cat = category.FirstOrDefault();
                        if (cat != null)
                        {
                            await this.ViewModelManager().ViewTransitionAsync(new Guid("B0737A90-696C-9155-D4C9-A628D1EED129"), cat);
                        }
                    }
                }

                FuncMenuCloseclickCommand();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private AsyncCommand _NetworkingParamPageSwitchingCommand;
        public ICommand NetworkingParamPageSwitchingCommand
        {
            get
            {
                if (null == _NetworkingParamPageSwitchingCommand) _NetworkingParamPageSwitchingCommand = new AsyncCommand(NetworkingParamPageSwitchingCommandFunc);
                return _NetworkingParamPageSwitchingCommand;
            }
        }
        private async Task NetworkingParamPageSwitchingCommandFunc()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (SystemCategories.Count < 1)
                {
                    InitSystemCategories();
                }

                retval = await CanChangeScreenForMaintenance();

                if (retval == EventCodeEnum.NONE)
                {
                    retval = await CanChangeScreenForManualSoaking();
                    if (retval == EventCodeEnum.NONE)
                    {
                        var category = SystemCategories.Where(c => c.Name == "Networking");
                        Category cat = category.FirstOrDefault();
                        if (cat != null)
                        {
                            await this.ViewModelManager().ViewTransitionAsync(new Guid("B0737A90-696C-9155-D4C9-A628D1EED129"), cat);
                        }
                    }
                }

                FuncMenuCloseclickCommand();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private AsyncCommand _ResultMapParamPageSwitchingCommand;
        public ICommand ResultMapParamPageSwitchingCommand
        {
            get
            {
                if (null == _ResultMapParamPageSwitchingCommand)
                    _ResultMapParamPageSwitchingCommand = new AsyncCommand(ResultMapParamPageSwitchingCommandFunc);
                return _ResultMapParamPageSwitchingCommand;
            }
        }
        private async Task ResultMapParamPageSwitchingCommandFunc()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (SystemCategories.Count < 1)
                {
                    InitSystemCategories();
                }

                retval = await CanChangeScreenForMaintenance();

                if (retval == EventCodeEnum.NONE)
                {
                    retval = await CanChangeScreenForManualSoaking();
                    if (retval == EventCodeEnum.NONE)
                    {
                        var category = SystemCategories.Where(c => c.Name == "Result Data");
                        Category cat = category.FirstOrDefault();
                        if (cat != null)
                        {
                            await this.ViewModelManager().ViewTransitionAsync(new Guid("B0737A90-696C-9155-D4C9-A628D1EED129"), cat);
                        }
                    }
                }

                FuncMenuCloseclickCommand();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private AsyncCommand _CardCleaningPageSwitchingCommand;
        public ICommand CardCleaningPageSwitchingCommand
        {
            get
            {


                if (null == _CardCleaningPageSwitchingCommand) _CardCleaningPageSwitchingCommand = new AsyncCommand(FuncCardCleaningPageSwitchingCommand);
                return _CardCleaningPageSwitchingCommand;
            }
        }
        private async Task FuncCardCleaningPageSwitchingCommand()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (DeviceCategories.Count < 1)
                {
                    InitDeviceCategories();
                }

                retval = await CanChangeScreenForMaintenance();

                if (retval == EventCodeEnum.NONE)
                {
                    retval = await CanChangeScreenForManualSoaking();
                    if (retval == EventCodeEnum.NONE)
                    {
                        var category = DeviceCategories.Where(c => c.Name == "Card Cleaning");
                        Category cat = category.FirstOrDefault();

                        if (cat != null)
                        {
                            await this.ViewModelManager().ViewTransitionAsync(new Guid("B0737A90-696C-9155-D4C9-A628D1EED129"), cat);
                        }
                    }
                }

                FuncMenuCloseclickCommand();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private AsyncCommand _ResultDataPageSwitchingCommand;
        public ICommand ResultDataPageSwitchingCommand
        {
            get
            {


                if (null == _ResultDataPageSwitchingCommand)
                    _ResultDataPageSwitchingCommand = new AsyncCommand(ResultDataPageSwitchingCommandFunc);
                return _ResultDataPageSwitchingCommand;
            }
        }
        private async Task ResultDataPageSwitchingCommandFunc()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (DeviceCategories.Count < 1)
                {
                    InitDeviceCategories();
                }

                retval = await CanChangeScreenForMaintenance();
                if (retval == EventCodeEnum.NONE)
                {
                    retval = await CanChangeScreenForManualSoaking();
                    if (retval == EventCodeEnum.NONE)
                    {
                        var category = DeviceCategories.Where(c => c.Name == "Result Data");
                        Category cat = category.FirstOrDefault();

                        if (cat != null)
                        {
                            await this.ViewModelManager().ViewTransitionAsync(new Guid("B0737A90-696C-9155-D4C9-A628D1EED129"), cat);
                        }
                    }
                }

                FuncMenuCloseclickCommand();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private AsyncCommand _DiagnosisPageSwitchingCommand;
        public ICommand DiagnosisPageSwitchingCommand
        {
            get
            {


                if (null == _DiagnosisPageSwitchingCommand) _DiagnosisPageSwitchingCommand = new AsyncCommand(FuncDiagnosisPageSwitchingCommand);
                return _DiagnosisPageSwitchingCommand;
            }
        }
        private async Task FuncDiagnosisPageSwitchingCommand()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (SystemCategories.Count < 1)
                {
                    InitSystemCategories();
                }
                CurCellPageGuid = CUIGuid;
                retval = await CanChangeScreenForMaintenance();

                if (retval == EventCodeEnum.NONE)
                {
                    retval = await CanChangeScreenForManualSoaking();
                    if (retval == EventCodeEnum.NONE)
                    {
                        var category = SystemCategories.Where(c => c.Name == "Diagnosis");
                        Category cat = category.FirstOrDefault();
                        if (cat != null)
                        {
                            await this.ViewModelManager().ViewTransitionAsync(new Guid("B0737A90-696C-9155-D4C9-A628D1EED129"), cat);
                        }
                    }
                }
                FuncMenuCloseclickCommand();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                CurCellPageGuid = ZeroNumsGuid;
            }
        }



        private AsyncCommand _LotPageSwitchingCommand;
        public ICommand LotPageSwitchingCommand
        {
            get
            {


                if (null == _LotPageSwitchingCommand) _LotPageSwitchingCommand = new AsyncCommand(FuncLotPageSwitchingCommand);
                return _LotPageSwitchingCommand;
            }
        }
        private async Task FuncLotPageSwitchingCommand()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (DeviceCategories.Count < 1)
                {
                    InitDeviceCategories();
                }

                retval = await CanChangeScreenForMaintenance();

                if (retval == EventCodeEnum.NONE)
                {
                    retval = await CanChangeScreenForManualSoaking();
                    if (retval == EventCodeEnum.NONE)
                    {
                        var category = DeviceCategories.Where(c => c.Name == "LOT");
                        Category cat = category.FirstOrDefault();
                        if (cat != null)
                        {
                            await this.ViewModelManager().ViewTransitionAsync(new Guid("B0737A90-696C-9155-D4C9-A628D1EED129"), cat);
                        }
                    }
                }

                FuncMenuCloseclickCommand();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private AsyncCommand _TemperaturePageSwitchingCommand;
        public ICommand TemperaturePageSwitchingCommand
        {
            get
            {


                if (null == _TemperaturePageSwitchingCommand) _TemperaturePageSwitchingCommand = new AsyncCommand(FuncTemperaturePageSwitchingCommand);
                return _TemperaturePageSwitchingCommand;
            }
        }
        private async Task FuncTemperaturePageSwitchingCommand()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (DeviceCategories.Count < 1)
                {
                    InitDeviceCategories();
                }

                retval = await CanChangeScreenForMaintenance();

                if (retval == EventCodeEnum.NONE)
                {
                    retval = await CanChangeScreenForManualSoaking();
                    if (retval == EventCodeEnum.NONE)
                    {
                        var category = DeviceCategories.Where(c => c.Name == "Temperature");
                        Category cat = category.FirstOrDefault();
                        if (cat != null)
                        {
                            await this.ViewModelManager().ViewTransitionAsync(new Guid("B0737A90-696C-9155-D4C9-A628D1EED129"), cat);
                        }
                    }
                }

                FuncMenuCloseclickCommand();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private Categories _SystemCategories = new Categories();

        public Categories SystemCategories
        {
            get { return _SystemCategories; }
            set { _SystemCategories = value; }
        }

        private Categories _DeviceCategories = new Categories();

        public Categories DeviceCategories
        {
            get { return _DeviceCategories; }
            set { _DeviceCategories = value; }
        }


        public async Task<EventCodeEnum> CanChangeScreenForMaintenance()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (loaderCommunicationManager.SelectedStage != null)
                {
                    if (CurCellPageGuid != ZeroNumsGuid)
                    {
                        bool iscontain = CardUpModuleCheckGuidList.Contains(CurCellPageGuid);
                        if (iscontain == true)
                        {
                            var iscardUpmoduleUp = LoaderMaster.IsCardUpmoduleUp();
                            if (iscardUpmoduleUp)
                            {
                                await this.MetroDialogManager().ShowMessageDialog("Can not switch page", "Card pod up module is up", MetroDialogInterfaces.EnumMessageStyle.Affirmative);
                                return EventCodeEnum.GP_CardChange_CHECK_TO_CARD_UP_MOUDLE;
                            }
                            else
                            {
                                retval = EventCodeEnum.NONE;
                            }
                        }
                    }

                    if (loaderCommunicationManager.SelectedStage.Reconnecting
                        || (loaderCommunicationManager.GetProxy<IRemoteMediumProxy>() == null) || (loaderCommunicationManager.GetProxy<IStageSupervisorProxy>() == null))
                    {
                        await this.MetroDialogManager().ShowMessageDialog(
                            "Error Message",
                            "The connection status of the stage is abnormal. Please check the stage connection.", EnumMessageStyle.Affirmative);

                        //await this.MetroDialogManager().ShowMessageDialog("Error Message", "The connection status of the stage is abnormal. Please check the stage connection.", EnumMessageStyle.Affirmative);

                        retval = EventCodeEnum.PARAM_ERROR;
                    }
                    else
                    {
                        if (loaderCommunicationManager.SelectedStage != null && (loaderCommunicationManager.SelectedStage as LoaderMapView.StageObject).ManualZUPState == ManualZUPStateEnum.Z_UP)
                        {
                            await this.MetroDialogManager().ShowMessageDialog(
                           "Error Message",
                           "You can move pages only when stage is in Z_Down Mode", EnumMessageStyle.Affirmative);
                            retval = EventCodeEnum.PARAM_ERROR;
                        }
                        else if (loaderCommunicationManager.SelectedStage.IsRecoveryMode == true)
                        {
                            await this.MetroDialogManager().ShowMessageDialog(
                         "Error Message",
                         "Page cannot be switched during recovery status.", EnumMessageStyle.Affirmative);
                            retval = EventCodeEnum.PARAM_ERROR;
                        }
                        else if (loaderCommunicationManager.SelectedStage.TCWMode == TCW_Mode.ON)
                        {
                            await this.MetroDialogManager().ShowMessageDialog(
                                   "Error Message",
                                   "You can move pages only when TCWMode Off. Current TCW Mode: ON", EnumMessageStyle.Affirmative);
                            retval = EventCodeEnum.PARAM_ERROR;
                            LoggerManager.Debug($"CanChangeScreenForMaintenance(): IsManualTeachPinMode true");
                        }
                        else if (loaderCommunicationManager.SelectedStage.StageMode == GPCellModeEnum.MAINTENANCE)
                        {
                            retval = EventCodeEnum.NONE;
                        }
                        else
                        {
                            if (AllowableOnlineModelist.Contains(CurCellPageGuid))
                            {
                                // PMiViewer에서 디바이스 정보를 사용하지 않게 변경됨으로써 Deviceload를 삭제하였음.

                                //var stage = loaderCommunicationManager.SelectedStage;
                                //await loaderCommunicationManager.DeviceReload(stage, true);

                                retval = EventCodeEnum.NONE;
                            }
                            else
                            {
                                await this.MetroDialogManager().ShowMessageDialog(
                                "Error Message",
                                "You can move pages only when stage is in maintanance mode.", EnumMessageStyle.Affirmative);

                                //await this.MetroDialogManager().ShowMessageDialog("Error Message", "You can move pages only when stage is in maintanance mode.", EnumMessageStyle.Affirmative);

                                retval = EventCodeEnum.PARAM_ERROR;
                            }
                        }
                    }
                }
                else
                {
                    await this.MetroDialogManager().ShowMessageDialog(
                                "Error Message",
                                "Please select a stage first.", EnumMessageStyle.Affirmative);

                    //await this.MetroDialogManager().ShowMessageDialog("Information", "Please select a stage first.", EnumMessageStyle.Affirmative);

                    retval = EventCodeEnum.PARAM_ERROR;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
        public async Task<EventCodeEnum> CanChangeScreenForManualSoaking()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (loaderCommunicationManager.SelectedStage != null && loaderCommunicationManager.SelectedStage.StageMode == GPCellModeEnum.MAINTENANCE)
                {
                    if (loaderCommunicationManager.SelectedStage.StageInfo.LotData.SoakingState == ModuleStateEnum.RUNNING.ToString()
                                              || loaderCommunicationManager.SelectedStage.StageInfo.LotData.SoakingState == ModuleStateEnum.SUSPENDED.ToString())
                    {
                        await this.MetroDialogManager().ShowMessageDialog(
                            "Error Message",
                            "Page cannot be switched during Manual Soaking status.", EnumMessageStyle.Affirmative);
                        retval = EventCodeEnum.PARAM_ERROR;
                        LoggerManager.SoakingLog($"CanChangeScreenForManualSoaking(): Page cannot be switched during Manual Soaking status.");
                    }
                    else
                    {
                        retval = EventCodeEnum.NONE;
                    }
                }
                else
                {
                    await this.MetroDialogManager().ShowMessageDialog(
                                "Error Message",
                                "Please select a stage first.", EnumMessageStyle.Affirmative);

                    //await this.MetroDialogManager().ShowMessageDialog("Information", "Please select a stage first.", EnumMessageStyle.Affirmative);

                    retval = EventCodeEnum.PARAM_ERROR;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public async Task<EventCodeEnum> CanChangeScreenForStageLock()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (loaderCommunicationManager.SelectedStage != null)
                {
                    if (LoaderMaster.GetClient(this.loaderCommunicationManager.SelectedStage.Index).GetStageLock() == StageLockMode.LOCK)
                    {
                        await this.MetroDialogManager().ShowMessageDialog(
                            "Error Message",
                            "Page cannot be switched during stage lock status.", EnumMessageStyle.Affirmative);
                        retval = EventCodeEnum.PARAM_ERROR;
                    }
                    else
                    {
                        retval = EventCodeEnum.NONE;
                    }
                }
                else
                {
                    await this.MetroDialogManager().ShowMessageDialog(
                                "Error Message",
                                "Please select a stage first.", EnumMessageStyle.Affirmative);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
        public async Task<EventCodeEnum> CanSwitchingPageForFoupRecovery()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                List<bool> foupenable = new List<bool>();
                //foup 확인하고 다 disable인 경우 page 진입 안되도록 해야 함.
                for (int i = 1; i <= SystemModuleCount.ModuleCnt.FoupCount; i++) // total foup count
                {
                    foupenable.Add(this.LoaderMaster.FoupOpModule().GetFoupController(i).FoupModuleInfo.Enable);
                }

                bool isAllFalse = foupenable.All(value => value == false);

                if (isAllFalse)
                {
                    retval = EventCodeEnum.NOT_EXIST_FOUP;
                }
                else
                {
                    retval = EventCodeEnum.NONE;
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public bool IsLoaderBusy()
        {
            bool retVal = false;
            try
            {
                if (this.LoaderMaster.ModuleState.State == ModuleStateEnum.RUNNING ||
                    this.LoaderMaster.ModuleState.State == ModuleStateEnum.ABORT ||
                    this.LoaderMaster.ModuleState.State == ModuleStateEnum.PAUSING)
                {
                    retVal = true;
                }
                else
                {
                    retVal = false;
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public EventCodeEnum InitSystemCategories()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                IViewModelManager ViewModelManager = this.ViewModelManager();
                SystemCategories = new Categories();


                foreach (var CategoryInfo in ViewModelManager.SystemSettingCategoryInfos)
                {
                    Category category = new Category();
                    List<string> descriptionList = new List<string>();
                    int i = 0;

                    category.Name = CategoryInfo.Name;
                    category.Icon = CategoryInfo.Icon;
                    category.Visibility = CategoryInfo.Visibility;
                    category.IsEnabled = CategoryInfo.IsEnabled;
                    category.MaskingLevel = CategoryInfo.MaskingLevel;

                    foreach (var SettingInfo in CategoryInfo.SettingInfos)
                    {
                        if (SettingInfo.CategoryID == "00010016" && SystemManager.SysteMode == SystemModeEnum.Multiple)
                            continue;
                        category.SettingViewInfos.Add(SettingInfo);

                        if (i < 3)
                        {
                            descriptionList.Add(SettingInfo.Name);
                            i++;
                        }
                    }

                    category.Description = string.Join(", ", descriptionList);
                    SystemCategories.Add(category);
                }
                retval = EventCodeEnum.NONE;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retval;
        }

        public EventCodeEnum InitDeviceCategories()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                IViewModelManager ViewModelManager = this.ViewModelManager();
                DeviceCategories = new Categories();

                foreach (var CategoryInfo in ViewModelManager.DeviceSettingCategoryInfos)
                {
                    Category category = new Category();
                    List<string> descriptionList = new List<string>();
                    int i = 0;

                    category.Name = CategoryInfo.Name;
                    category.Icon = CategoryInfo.Icon;
                    category.Visibility = CategoryInfo.Visibility;
                    category.IsEnabled = CategoryInfo.IsEnabled;
                    category.MaskingLevel = CategoryInfo.MaskingLevel;

                    foreach (var SettingInfo in CategoryInfo.SettingInfos)
                    {
                        category.SettingViewInfos.Add(SettingInfo);

                        //if (i < 3)
                        //{
                        //    descriptionList.Add(SettingInfo.Name);
                        //    i++;
                        //}
                        descriptionList.Add(SettingInfo.Name);
                    }

                    category.Description = string.Join(", ", descriptionList);
                    DeviceCategories.Add(category);
                }
                retval = EventCodeEnum.NONE;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retval;
        }

        //private AsyncCommand _MotParamPageSwitchingCommand;
        //public ICommand MotParamPageSwitchingCommand
        //{
        //    get
        //    {
        //        if (null == _MotParamPageSwitchingCommand) _MotParamPageSwitchingCommand = new AsyncCommand(FuncMotParamPageSwitchingCommand);
        //        return _MotParamPageSwitchingCommand;
        //    }
        //}
        //private async Task FuncMotParamPageSwitchingCommand()
        //{
        //    try
        //    {
        //        await this.ViewModelManager().ViewTransitionAsync(new Guid("7bd6a1b7-8d90-4dee-8de7-633bb966de6a"));
        //        FuncMenuCloseclickCommand();
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //    }
        //}

        private AsyncCommand _FileTransferPagePageSwitchingCommand;
        public ICommand FileTransferPagePageSwitchingCommand
        {
            get
            {
                if (null == _FileTransferPagePageSwitchingCommand) _FileTransferPagePageSwitchingCommand = new AsyncCommand(FuncFileTransferPagePageSwitchingCommand);
                return _FileTransferPagePageSwitchingCommand;
            }
        }
        private async Task FuncFileTransferPagePageSwitchingCommand()
        {
            try
            {
                await this.ViewModelManager().ViewTransitionAsync(new Guid("26e6e48b-d06a-43f9-9efc-efd1a21ea493"));
                FuncMenuCloseclickCommand();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private AsyncCommand _ManualPageSwitchingCommand;
        public ICommand ManualPageSwitchingCommand
        {
            get
            {
                if (null == _ManualPageSwitchingCommand) _ManualPageSwitchingCommand = new AsyncCommand(ManuaPPageSwitchingCommandFunc);
                return _ManualPageSwitchingCommand;
            }
        }
        private async Task ManuaPPageSwitchingCommandFunc()
        {
            try
            {
                await this.ViewModelManager().ViewTransitionAsync(HandlingViewGuid);

                FuncMenuCloseclickCommand();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }


        private AsyncCommand _OperationPageSwitchingCommand;
        public ICommand OperationPageSwitchingCommand
        {
            get
            {
                if (null == _OperationPageSwitchingCommand) _OperationPageSwitchingCommand = new AsyncCommand(OperationPageSwitchingCommandFunc);
                return _OperationPageSwitchingCommand;
            }
        }
        private async Task OperationPageSwitchingCommandFunc()
        {
            try
            {
                string text = null;

                if (!IsLoaderBusy())
                {
                    System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        text = VirtualKeyboard.Show(text, KB_TYPE.ALPHABET | KB_TYPE.PASSWORD);
                        String superPassword = AccountManager.MakeSuperAccountPassword();

                        if (text.ToLower().CompareTo(superPassword) == 0)
                        {
                            this.ViewModelManager().ViewTransitionAsync(new Guid("4732F634-2292-6228-C7E5-24A18C888187"));
                        }
                    }));
                }
                else
                {
                    await this.MetroDialogManager().ShowMessageDialog("[Information]", $"Loader State is {LoaderMaster.ModuleState.State.ToString()} State. Please Check the Loader State.", EnumMessageStyle.Affirmative);
                }

                FuncMenuCloseclickCommand();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        private AsyncCommand _SetupWithCellCommand;
        public ICommand SetupWithCellCommand
        {
            get
            {
                if (null == _SetupWithCellCommand) _SetupWithCellCommand = new AsyncCommand(SetupWithCellCommandFunc);
                return _SetupWithCellCommand;
            }
        }
        private async Task SetupWithCellCommandFunc()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;
            try
            {
                CurCellPageGuid = SetupWithCellGuid;
                retval = await CanChangeScreenForMaintenance();
                if (retval == EventCodeEnum.NONE)
                {
                    retval = await CanChangeScreenForManualSoaking();
                    if (retval == EventCodeEnum.NONE)
                    {
                        retval = await CanChangeScreenForStageLock();
                        if(retval == EventCodeEnum.NONE)
                        {
                            await this.ViewModelManager().ViewTransitionAsync(new Guid("5FBF9EB1-C022-4FE6-A665-6569D45EFD3A"));
                        }
                    }
                    FuncMenuCloseclickCommand();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                CurCellPageGuid = ZeroNumsGuid;
            }
        }


        private AsyncCommand _ManualContactPageSwitchingCommand;
        public ICommand ManualContactPageSwitchingCommand
        {
            get
            {
                if (null == _ManualContactPageSwitchingCommand) _ManualContactPageSwitchingCommand = new AsyncCommand(ManualContactPageSwitchingCommandFunc);
                return _ManualContactPageSwitchingCommand;
            }
        }
        private async Task ManualContactPageSwitchingCommandFunc()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                CurCellPageGuid = ManualContactGuid;
                retval = await CanChangeScreenForMaintenance();

                if (retval == EventCodeEnum.NONE)
                {
                    retval = await CanChangeScreenForManualSoaking();
                    if (retval == EventCodeEnum.NONE)
                    {
                        retval = await CanChangeScreenForStageLock();
                        if (retval == EventCodeEnum.NONE)
                        {
                            bool WaferAligned = false;
                            bool PinAligned = false;
                            string waferdatavalid = string.Empty;
                            string pindatavalid = string.Empty;

                            Element<AlignStateEnum> WaferAlignState = loaderCommunicationManager.GetProxy<IStageSupervisorProxy>().GetAlignState(AlignTypeEnum.Wafer);

                            if (WaferAlignState.Value == AlignStateEnum.DONE)
                            {
                                WaferAligned = true;
                                waferdatavalid = "is enough.";
                            }
                            else
                            {
                                waferdatavalid = "is not enough.";
                            }

                            Element<AlignStateEnum> PinAlignState = loaderCommunicationManager.GetProxy<IStageSupervisorProxy>().GetAlignState(AlignTypeEnum.Pin);

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
                                await this.ViewModelManager().ViewTransitionAsync(new Guid("ac247488-2cb3-4250-9cfd-bd6852802a83"));
                                // await this.ViewModelManager().ViewTransitionAsync(new Guid("6c307199-ddcd-496d-89f7-a462bd13f949"));
                            }
                            else
                            {
                                await this.MetroDialogManager().ShowMessageDialog(
                                    "[Information]", $"There is not enough data required.\nWafer Align data {waferdatavalid}\nPin Align data {pindatavalid}", EnumMessageStyle.Affirmative);

                                //await this.MetroDialogManager().ShowMessageDialog("[Information]",
                                //    $"There is not enough data required.\nWafer Align data {waferdatavalid}\nPin Align data {pindatavalid}",
                                //    EnumMessageStyle.Affirmative);
                            }
                        }
                    }
                }
                FuncMenuCloseclickCommand();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                CurCellPageGuid = ZeroNumsGuid;
            }
        }



        private AsyncCommand _MultiManualContactPageSwitchingCommand;
        public ICommand MultiManualContactPageSwitchingCommand
        {
            get
            {
                if (null == _MultiManualContactPageSwitchingCommand) _MultiManualContactPageSwitchingCommand = new AsyncCommand(MultiManualContactPageSwitchingCommandFunc);
                return _MultiManualContactPageSwitchingCommand;
            }
        }
        private async Task MultiManualContactPageSwitchingCommandFunc()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                retval = await CanChangeScreenForManualSoaking();
                if (retval == EventCodeEnum.NONE)
                {
                    if (!IsLoaderBusy())
                    {
                        await this.ViewModelManager().ViewTransitionAsync(new Guid("6c307199-ddcd-496d-89f7-a462bd13f949"));
                    }
                    else
                    {
                        await this.MetroDialogManager().ShowMessageDialog(
                            "[Information]", $"Loader State is {LoaderMaster.ModuleState.State.ToString()} State. Please Check the Loader State.", EnumMessageStyle.Affirmative);
                    }
                }
                FuncMenuCloseclickCommand();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                CurCellPageGuid = ZeroNumsGuid;
            }
        }

        private AsyncCommand _TechnologiesPageSwitchingCommand;
        public ICommand TechnologiesPageSwitchingCommand
        {
            get
            {
                if (null == _TechnologiesPageSwitchingCommand) _TechnologiesPageSwitchingCommand = new AsyncCommand(TechnologiesPageSwitchingCommandFunc);
                return _TechnologiesPageSwitchingCommand;
            }
        }
        private async Task TechnologiesPageSwitchingCommandFunc()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                retval = await CanChangeScreenForMaintenance();

                if (retval == EventCodeEnum.NONE)
                {
                    await this.ViewModelManager().ViewTransitionAsync(new Guid("3E34A0EA-05F2-029E-6D2D-DFCF43922121"));

                }

                FuncMenuCloseclickCommand();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private AsyncCommand _WaferInspectionPageSwitchingCommand;
        public ICommand WaferInspectionPageSwitchingCommand
        {
            get
            {
                if (null == _WaferInspectionPageSwitchingCommand) _WaferInspectionPageSwitchingCommand = new AsyncCommand(WaferInspectionPageSwitchingCommandFunc);
                return _WaferInspectionPageSwitchingCommand;
            }
        }
        private async Task WaferInspectionPageSwitchingCommandFunc()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                CurCellPageGuid = WaferInspectionGuid;
                retval = await CanChangeScreenForMaintenance();

                if (retval == EventCodeEnum.NONE)
                {
                    retval = await CanChangeScreenForManualSoaking();
                    if (retval == EventCodeEnum.NONE)
                    {
                        retval = await CanChangeScreenForStageLock();
                        if (retval == EventCodeEnum.NONE)
                        {
                            await this.ViewModelManager().ViewTransitionAsync(new Guid("f8396e3a-b8ce-4dcd-9a0d-643532a7d9d1"));
                        }
                    }
                }
                FuncMenuCloseclickCommand();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                CurCellPageGuid = ZeroNumsGuid;
            }
        }

        private AsyncCommand _LoaderParameterPageSwitchingCommand;
        public ICommand LoaderParameterPageSwitchingCommand
        {
            get
            {
                if (null == _LoaderParameterPageSwitchingCommand) _LoaderParameterPageSwitchingCommand = new AsyncCommand(LoaderParameterPageSwitchingCommandFunc);
                return _LoaderParameterPageSwitchingCommand;
            }
        }
        private async Task LoaderParameterPageSwitchingCommandFunc()
        {
            try
            {
                string text = null;

                if (!IsLoaderBusy())
                {
                    System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        text = VirtualKeyboard.Show(text, KB_TYPE.ALPHABET | KB_TYPE.PASSWORD);
                        String superPassword = AccountManager.MakeSuperAccountPassword();

                        if (text.ToLower().CompareTo(superPassword) == 0)
                        {
                            this.ViewModelManager().ViewTransitionAsync(new Guid("AE2B4076-1C65-87E1-7DA5-64BA7B1D4CCA"));
                        }
                    }));
                }
                else
                {
                    this.MetroDialogManager().ShowMessageDialog("[Information]", $"Loader State is {LoaderMaster.ModuleState.State.ToString()} State. Please Check the Loader State.", EnumMessageStyle.Affirmative);
                }

                FuncMenuCloseclickCommand();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private AsyncCommand _ConnectPageSwitchingCommand;
        public ICommand ConnectPageSwitchingCommand
        {
            get
            {
                if (null == _ConnectPageSwitchingCommand) _ConnectPageSwitchingCommand = new AsyncCommand(ConnectPageSwitchingCommandFunc);
                return _ConnectPageSwitchingCommand;
            }
        }
        private async Task ConnectPageSwitchingCommandFunc()
        {
            try
            {
                await this.ViewModelManager().ViewTransitionAsync(new Guid("E255E260-E77F-900E-B463-BDA36F5E08ED"));
                FuncMenuCloseclickCommand();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private AsyncCommand _SecsGemSettingDialogOpenCommand;
        public ICommand SecsGemSettingDialogOpenCommand
        {
            get
            {
                if (null == _SecsGemSettingDialogOpenCommand) _SecsGemSettingDialogOpenCommand = new AsyncCommand(SecsGemSettingDialogOpenCommandFunc);
                return _SecsGemSettingDialogOpenCommand;
            }
        }

        private SecsGemSettingDialog _SecsGemDialog { get; set; }
        private async Task SecsGemSettingDialogOpenCommandFunc()
        {
            try
            {

                System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (_SecsGemDialog != null)
                    {
                        _SecsGemDialog.Close();
                        _SecsGemDialog = null;
                    }
                    _SecsGemDialog = new SecsGemSettingDialog(false);
                    _SecsGemDialog.Width = 800;
                    _SecsGemDialog.Height = 680;
                    _SecsGemDialog.Title = "Gem";
                    _SecsGemDialog.Topmost = true;
                    _SecsGemDialog.Show();
                    FuncMenuCloseclickCommand();
                }));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private AsyncCommand _ChillerPageSwitchingCommand;
        public ICommand ChillerPageSwitchingCommand
        {
            get
            {
                if (null == _ChillerPageSwitchingCommand) _ChillerPageSwitchingCommand = new AsyncCommand(ChillerPageSwitchingCommandFunc);
                return _ChillerPageSwitchingCommand;
            }
        }
        private async Task ChillerPageSwitchingCommandFunc()
        {
            try
            {
                if (SystemManager.SystemType == SystemTypeEnum.DRAX)
                {
                    await this.ViewModelManager().ViewTransitionAsync(new Guid("3817ed23-c61a-47a1-8e97-6fc2c3a210c4"));
                }
                else
                {
                    await this.ViewModelManager().ViewTransitionAsync(new Guid("7797A3B8-5CF5-FCCD-22BB-F612618C4B34"));
                }
                FuncMenuCloseclickCommand();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }


        private AsyncCommand _OpenFileExplorerCommand;
        public ICommand OpenFileExplorerCommand
        {
            get
            {
                if (null == _OpenFileExplorerCommand) _OpenFileExplorerCommand = new AsyncCommand(OpenFileExplorerCommandFunc);
                return _OpenFileExplorerCommand;
            }
        }
        private async Task OpenFileExplorerCommandFunc()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                string path = @"C://";
                Process.Start(path);
                FuncMenuCloseclickCommand();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }


        private AsyncCommand _ExitClickCommand;
        public ICommand ExitClickCommand
        {
            get
            {
                if (null == _ExitClickCommand) _ExitClickCommand = new AsyncCommand(ExitClickCommandFunc);
                return _ExitClickCommand;
            }
        }


        private async Task ExitClickCommandFunc()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (!IsLoaderBusy())
                {
                    var result = this.MetroDialogManager().ShowMessageDialog(
                          "Program Exit",
                          "Do you want to exit the program?", EnumMessageStyle.AffirmativeAndNegative);

                    if (result.Result == EnumMessageDialogResult.AFFIRMATIVE)
                    {

                        Window w = null;
                        IReleaseResource disposable = null;
                        System.Windows.Application.Current.Dispatcher.Invoke(() =>
                        {
                            w = System.Windows.Application.Current.MainWindow;
                            disposable = System.Windows.Application.Current.MainWindow as IReleaseResource;
                        });

                        LoaderMaster.StatusSoakingUpdateInfoStop = true;
                        LoaderMaster.StageWatchDogStop = true;
                        disposable.Release();
                        Extensions_IParam.ProgramShutDown = true;
                    }
                }
                else
                {
                    await this.MetroDialogManager().ShowMessageDialog("[Information]", $"Loader State is {LoaderMaster.ModuleState.State.ToString()} State. Please Check the Loader State.", EnumMessageStyle.Affirmative);
                }

                FuncMenuCloseclickCommand();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion

        #endregion



        private AsyncCommand _MarkSetupPageSwitchingCommand;
        public ICommand MarkSetupPageSwitchingCommand
        {
            get
            {
                if (null == _MarkSetupPageSwitchingCommand) _MarkSetupPageSwitchingCommand = new AsyncCommand(MarkSetupPageSwitchingCommandFunc);
                return _MarkSetupPageSwitchingCommand;
            }
        }



        private async Task MarkSetupPageSwitchingCommandFunc()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (DeviceCategories.Count < 1)
                {
                    InitDeviceCategories();
                }

                retval = await CanChangeScreenForMaintenance();

                if (retval == EventCodeEnum.NONE)
                {
                    retval = await CanChangeScreenForManualSoaking();
                    if (retval == EventCodeEnum.NONE)
                    {
                        var category = DeviceCategories.Where(c => c.Name == "Wafer");
                        Category cat = category.FirstOrDefault();
                        if (cat != null)
                        {
                            await this.ViewModelManager().ViewTransitionAsync(new Guid("B0737A90-696C-9155-D4C9-A628D1EED129"), cat);
                        }
                    }
                }

                FuncMenuCloseclickCommand();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {

            }
        }


        #region <remarks> VerifyParamPageSwitchingCommand </remarks>

        private AsyncCommand _VerifyParamPageSwitchingCommand;
        public ICommand VerifyParamPageSwitchingCommand
        {
            get
            {
                if (null == _VerifyParamPageSwitchingCommand) _VerifyParamPageSwitchingCommand = new AsyncCommand(VerifyParamPageSwitchingCommandFunc);
                return _VerifyParamPageSwitchingCommand;
            }
        }

        private async Task VerifyParamPageSwitchingCommandFunc()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                string text = null;

                System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    text = VirtualKeyboard.Show(text, KB_TYPE.ALPHABET | KB_TYPE.PASSWORD);
                    String superPassword = AccountManager.MakeSuperAccountPassword();

                    if (text.ToLower().CompareTo(superPassword) == 0)
                    {
                        this.ViewModelManager().ViewTransitionAsync(new Guid("23b429a7-1ce8-4897-9eeb-d01b6a106714"));
                        FuncMenuCloseclickCommand();
                    }
                    else
                    {
                        this.MetroDialogManager().ShowMessageDialog("Warning Message", "Passwords do not match.", EnumMessageStyle.Affirmative);
                    }
                }));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion

        #region <remarks> E84PageSwitchingCommand </remarks>

        private AsyncCommand _E84PageSwitchingCommand;
        public ICommand E84PageSwitchingCommand
        {
            get
            {
                if (null == _E84PageSwitchingCommand) _E84PageSwitchingCommand = new AsyncCommand(E84PageSwitchingCommandFunc);
                return _E84PageSwitchingCommand;
            }
        }

        private async Task E84PageSwitchingCommandFunc()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                await this.ViewModelManager().ViewTransitionAsync(new Guid("1698b29e-f5f4-4597-8ac7-974bb4fbd79e"));
                FuncMenuCloseclickCommand();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion

        #region <remarks> UtilityOptionSwitchingCommand </remarks>

        private AsyncCommand _UtilityOptionSwitchingCommand;
        public ICommand UtilityOptionSwitchingCommand
        {
            get
            {
                if (null == _UtilityOptionSwitchingCommand) _UtilityOptionSwitchingCommand = new AsyncCommand(UtilityOptionSwitchingCommandFunc);
                return _UtilityOptionSwitchingCommand;
            }
        }

        private async Task UtilityOptionSwitchingCommandFunc()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                string text = null;

                System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    text = VirtualKeyboard.Show(text, KB_TYPE.ALPHABET | KB_TYPE.PASSWORD);
                    String superPassword = AccountManager.MakeSuperAccountPassword();

                    if (text.ToLower().CompareTo(superPassword) == 0)
                    {
                        this.ViewModelManager().ViewTransitionAsync(new Guid("386e667d-046c-415a-b383-12c40902e5cb"));
                        FuncMenuCloseclickCommand();
                    }
                }));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion

        #region <remarks> FoupRecoverySwitchingCommand </remarks>

        private AsyncCommand _FoupRecoverySwitchingCommand;
        public ICommand FoupRecoverySwitchingCommand
        {
            get
            {
                if (null == _FoupRecoverySwitchingCommand) _FoupRecoverySwitchingCommand = new AsyncCommand(FoupRecoverySwitchingCommandFunc);
                return _FoupRecoverySwitchingCommand;
            }
        }

        private async Task FoupRecoverySwitchingCommandFunc()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                string text = null;
                //Lot 중인지 확인.

                retval = await CanSwitchingPageForFoupRecovery();
                
                if (retval == EventCodeEnum.NONE)
                {
                    ModuleStateEnum loaderstate = this.LoaderMaster.ModuleState.State;
                    if (loaderstate == ModuleStateEnum.IDLE || loaderstate == ModuleStateEnum.PAUSED)
                    {
                        System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            this.ViewModelManager().ViewTransitionAsync(FoupRecoveryViewGuid);
                            FuncMenuCloseclickCommand();
                        }));
                    }
                    else
                    {
                        this.MetroDialogManager().ShowMessageDialog($"Loader State {loaderstate}", "You can enter the page only when the loader status is IDLE or PASUED.", EnumMessageStyle.Affirmative, "OK");
                    }
                }
                else
                {
                    this.MetroDialogManager().ShowMessageDialog("Not all foups are available.", "You can enter the page only when there is at least one available foup.", EnumMessageStyle.Affirmative, "OK");
                }
                
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion

        #region <remarks> EnvMonitoringPageSwitchingCommand </remarks>        
        private AsyncCommand _EnvMonitoringPageSwitchingCommand;
        public ICommand EnvMonitoringPageSwitchingCommand
        {
            get
            {
                if (null == _EnvMonitoringPageSwitchingCommand) _EnvMonitoringPageSwitchingCommand = new AsyncCommand(EnvMonitoringPageSwitchingCommandFunc);
                return _EnvMonitoringPageSwitchingCommand;
            }
        }

        private async Task EnvMonitoringPageSwitchingCommandFunc()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                await this.ViewModelManager().ViewTransitionAsync(new Guid("BF53C480-8E52-4228-80E4-449E62EA8AA7"));
                FuncMenuCloseclickCommand();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion


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
            return Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);
        }

        public Task<EventCodeEnum> DeInitViewModel(object parameter = null)
        {
            return Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);
        }
    }
}

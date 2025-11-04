using System;
using System.Threading.Tasks;

namespace PolishWaferDevMainPageViewModel
{
    using LogModule;
    using PolishWaferDevMainPageViewModel.WaferSelection;
    using ProberErrorCode;
    using ProberInterfaces;
    using RelayCommandBase;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Windows.Input;
    using ManualSetting;
    using SettingMainViewModel;

    public class VmPolishWaferDevMainPage : IMainScreenViewModel
    {
        #region //..IMainScreenViewModel Property
        public bool Initialized { get; set; } = false;
        private readonly Guid _ViewModelGUID = new Guid("B901EE43-BCCC-B4F4-8167-562CD2EFDACA");
        public Guid ScreenGUID
        {
            get { return _ViewModelGUID; }
        }
        #endregion

        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged([CallerMemberName]string propName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }


        #endregion

        #region //..Creator & IMainScreenViewModel Method
        public VmPolishWaferDevMainPage()
        {
        }
        public EventCodeEnum InitModule()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (Initialized == false)
                {
                    Initialized = true;

                    retVal = EventCodeEnum.NONE;
                }
                else
                {
                    LoggerManager.Error($"DUPLICATE_INVOCATION");

                    retVal = EventCodeEnum.DUPLICATE_INVOCATION;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public void DeInitModule()
        {
            return;
        }


        public Task<EventCodeEnum> InitViewModel()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                WaferObject = this.GetParam_Wafer();
                _WaferSelectionViewModel = new WaferSelectionViewModel();
                _ManualSettingViewModel = new ManualSettingViewModel();
                _Centering_Focusing_SetupViewModel = new Centering_Focusing_SetupViewModel();
                _Cleaning_SetupViewModel = new Cleaning_SetupViewModel();
                _Wafer_SetupViewModel = new Wafer_SetupViewModel();
                _Manual_SetupViewModel = new Manual_SetupViewModel();

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

        public Task<EventCodeEnum> PageSwitched(object parameter = null)
        {
            SelectedStepGuid = new Guid("75C98D02-9854-7723-C912-1D89EE60A24F");
            return Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);
        }

        public Task<EventCodeEnum> Cleanup(object parameter = null)
        {
            return Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);
        }
        #endregion

        #region //..Property
        public WaferSelectionViewModel _WaferSelectionViewModel { get; set; }
        public ManualSettingViewModel _ManualSettingViewModel { get; set; }

        public Centering_Focusing_SetupViewModel _Centering_Focusing_SetupViewModel { get; set; }
        public Cleaning_SetupViewModel _Cleaning_SetupViewModel { get; set; }
        public Wafer_SetupViewModel _Wafer_SetupViewModel { get; set; }
        public Manual_SetupViewModel _Manual_SetupViewModel { get; set; }


        private Guid _SelectedStepGuid;
        public Guid SelectedStepGuid
        {
            get { return _SelectedStepGuid; }
            set
            {
                if (value != _SelectedStepGuid)
                {
                    _SelectedStepGuid = value;
                    RaisePropertyChanged();
                }
            }
        }

        public IWaferObject WaferObject { get; set; }


        #endregion

        #region //..Command & Command Method
        private AsyncCommand<CUI.Button> _ChangeSetupSubScreenCommand;
        public ICommand ChangeSetupSubScreenCommand
        {
            get
            {
                if (null == _ChangeSetupSubScreenCommand) _ChangeSetupSubScreenCommand
                        = new AsyncCommand<CUI.Button>(FucnChangeSetupSubScreenCommand);
                return _ChangeSetupSubScreenCommand;
            }
        }
        private async Task FucnChangeSetupSubScreenCommand(CUI.Button button)
        {
            try
            {
                EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
                Guid guid = button.GUID;

                if (guid.ToString().Equals("75c98d02-9854-7723-c912-1d89ee60a24f"))
                    retVal = await _Centering_Focusing_SetupViewModel.PageSwitched();
                else if (guid.ToString().Equals("71c18d46-06e8-06da-e367-54ffa17e9dbf"))
                    retVal = await _Cleaning_SetupViewModel.PageSwitched();
                else if (guid.ToString().Equals("2fdaae5a-3d48-867c-b561-fdce592ae346"))
                    retVal = await _Wafer_SetupViewModel.PageSwitched();
                else if (guid.ToString().Equals("3cb80428-23b8-922b-fbda-f9f3b04df9b0"))
                    retVal = await _Manual_SetupViewModel.PageSwitched();

                if(retVal == EventCodeEnum.NONE)
                    SelectedStepGuid = button.GUID;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

        }
        #endregion

    }
}

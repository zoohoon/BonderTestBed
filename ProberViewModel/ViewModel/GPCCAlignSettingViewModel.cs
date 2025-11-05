using System;
using System.Threading.Tasks;

namespace GPCCAlignSettingViewModel_Standard
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Windows.Input;
    using CUIServices;
    using LogModule;
    using ProberErrorCode;
    using ProberInterfaces;
    using RelayCommandBase;

    public class GPCCAlignSettingViewModel : IMainScreenViewModel
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged([CallerMemberName]string propName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }
        #endregion

        private readonly Guid _ViewModelGUID = new Guid("E98E0998-7F24-4EFC-96C9-D00A9D9EDF38");
        public Guid ScreenGUID
        {
            get { return _ViewModelGUID; }
        }

        public bool Initialized { get; set; } = false;

        #region ==> SetupCommand
        private AsyncCommand<CUI.Button> _SetupCommand;
        public ICommand SetupCommand
        {
            get
            {
                if (null == _SetupCommand) _SetupCommand = new AsyncCommand<CUI.Button>(SetupCommandFunc);
                return _SetupCommand;
            }
        }
        private async Task SetupCommandFunc(CUI.Button cuiparam)
        {
            this.StageSupervisor().StageModuleState.ZCLEARED();

            StageStateEnum state = this.StageSupervisor().StageModuleState.GetState();
            
            if (state != StageStateEnum.IDLE &&
                state != StageStateEnum.Z_CLEARED && 
                state != StageStateEnum.MOVETOLOADPOS &&
                state != StageStateEnum.CARDCHANGE)
            {
                return;
            }

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
        public void DeInitModule()
        {

        }
        public EventCodeEnum InitModule()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

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

            return retval;
        }
    }
}

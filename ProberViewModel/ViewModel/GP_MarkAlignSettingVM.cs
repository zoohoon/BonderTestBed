using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using CUIServices;
using LogModule;
using ProberErrorCode;
using ProberInterfaces;
using RelayCommandBase;

namespace GP_MarkAlignSettingViewModel
{
    public class GP_MarkAlignSettingVM : IMainScreenViewModel
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        readonly Guid _ViewModelGUID = new Guid("0f6b9b1f-ea13-4241-87e0-b4193dea5ae5");
        public Guid ScreenGUID { get { return _ViewModelGUID; } }

        public bool Initialized { get; set; } = false;

        public Task<EventCodeEnum> DeInitViewModel(object parameter = null)
        {
            return Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);
        }

        public EventCodeEnum InitModule()
        {
            try
            {

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return EventCodeEnum.NONE;
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


        public async Task<EventCodeEnum> InitViewModel()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public Task<EventCodeEnum> PageSwitched(object parameter = null)
        {
            return Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);
        }
        public Task<EventCodeEnum> Cleanup(object parameter = null)
        {
            return Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);
        }

        private AsyncCommand<CUI.Button> _MarkAlignSetupCommand;
        public ICommand MarkAlignSetupCommand
        {
            get
            {
                if (null == _MarkAlignSetupCommand) _MarkAlignSetupCommand = new AsyncCommand<CUI.Button>(MarkAlignSetupCommandFunc);
                return _MarkAlignSetupCommand;
            }
        }

        private async Task MarkAlignSetupCommandFunc(CUI.Button cuiparam)
        {
            try
            {
                Guid ViewGUID = CUIService.GetTargetViewGUID(cuiparam.GUID);
                await this.ViewModelManager().ViewTransitionAsync(ViewGUID);
                this.PnPManager().SettingRemotePNP("MarkAlign", "IMarkAligner", new Guid("66ae4fca-caf5-42b9-a4ba-bd22d026e65a"));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }


    }
}

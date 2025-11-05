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

namespace ChuckPlanaritySubSettingViewModel
{
    public class ChuckPlanaritySubSettingVM : IMainScreenViewModel
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        readonly Guid _ViewModelGUID = new Guid("2c50095f-50b0-4984-8208-d913f5a2b3fd");
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

        private RelayCommand<CUI.Button> _PageSwitchingCommand;
        public ICommand PageSwitchingCommand
        {
            get
            {
                if (null == _PageSwitchingCommand) _PageSwitchingCommand = new RelayCommand<CUI.Button>(PageSwitchingCommandFunc);
                return _PageSwitchingCommand;
            }
        }

        private void PageSwitchingCommandFunc(CUI.Button cuiparam)
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

        

    }
}

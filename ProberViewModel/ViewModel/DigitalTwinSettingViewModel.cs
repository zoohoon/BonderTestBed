using CUIServices;
using LogModule;
using ProberErrorCode;
using ProberInterfaces;
using RelayCommandBase;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ProberViewModel.ViewModel
{
    public class DigitalTwinSettingViewModel : IMainScreenViewModel, INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion


        #region IMainScreenViewModel

        public bool Initialized { get; set; } = false;

        readonly Guid _ViewGUID = new Guid("9be725b8-2a9e-47c1-9c1c-c18671cc52d6");
        public Guid ScreenGUID { get { return _ViewGUID; } }

        public Task<EventCodeEnum> InitViewModel()
        {
            Task<EventCodeEnum> retval = null;

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
            Task<EventCodeEnum> retval = null;

            try
            {
                retval = Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public Task<EventCodeEnum> Cleanup(object parameter = null)
        {
            Task<EventCodeEnum> retval = null;

            try
            {
                retval = Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public Task<EventCodeEnum> DeInitViewModel(object parameter = null)
        {
            Task<EventCodeEnum> retval = null;

            try
            {

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
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

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
        #endregion

        #region //..Command 
        private RelayCommand<CUI.Button> _SetupCommand;
        public ICommand SetupCommand
        {
            get
            {
                if (null == _SetupCommand) _SetupCommand = new RelayCommand<CUI.Button>(FuncSetupCommand);
                return _SetupCommand;
            }
        }

        private void FuncSetupCommand(CUI.Button cuiparam)
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
    }
}

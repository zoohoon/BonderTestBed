using System;
using System.Threading.Tasks;

namespace PolishWaferDevMainPageViewModel.SettingMainViewModel
{
    using LogModule;
    using ProberErrorCode;
    using ProberInterfaces;
    using ProberInterfaces.PolishWafer;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    public class Manual_SetupViewModel : INotifyPropertyChanged, IFactoryModule, IIPolishWaferSetupViewModel
    {
        #region == > PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region //..Property
        public IWaferObject WaferObject { get; set; }
        #endregion

        public Manual_SetupViewModel()
        {
            WaferObject = this.GetParam_Wafer();
        }
        public Task<EventCodeEnum> PageSwitched()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return Task.FromResult<EventCodeEnum>(retVal);
        }

    }
}

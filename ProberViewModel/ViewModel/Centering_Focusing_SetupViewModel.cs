using System;
using System.Threading.Tasks;

namespace PolishWaferDevMainPageViewModel.SettingMainViewModel
{
    using LogModule;
    using ProberErrorCode;
    using ProberInterfaces;
    using ProberInterfaces.LightJog;
    using ProberInterfaces.PolishWafer;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class Centering_Focusing_SetupViewModel : INotifyPropertyChanged, IFactoryModule, IUseLightJog, IIPolishWaferSetupViewModel
    {
        #region == > PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        private ICamera _CurCam;
        public ICamera CurCam
        {
            get { return _CurCam; }
            set
            {
                if (value != _CurCam)
                {
                    _CurCam = value;
                    RaisePropertyChanged();
                }
            }
        }


        //==> Light Jog
        public LightJogViewModel LightJog { get; set; }

        public Centering_Focusing_SetupViewModel()
        {
            try
            {
                CurCam = this.VisionManager().GetCam(EnumProberCam.WAFER_LOW_CAM);
                this.VisionManager().StartGrab(CurCam.GetChannelType(), this);

                LightJog = new LightJogViewModel(maxLightValue: 255, minLightValue: 0);
                LightJog.InitCameraJog(this);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }

        }

        public Task<EventCodeEnum> PageSwitched()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                CurCam = this.VisionManager().GetCam(EnumProberCam.WAFER_LOW_CAM);
                this.VisionManager().StartGrab(CurCam.GetChannelType(), this);

                LightJog.InitCameraJog(this);
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

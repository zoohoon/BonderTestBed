using System;
using System.Threading.Tasks;

namespace PolishWaferDevMainPageViewModel.SettingMainViewModel
{
    using LogModule;
    using PolishWaferParameters;
    using ProberErrorCode;
    using ProberInterfaces;
    using ProberInterfaces.PolishWafer;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    public class Cleaning_SetupViewModel : INotifyPropertyChanged, IFactoryModule, IIPolishWaferSetupViewModel
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

        private Element<EnumCleaningTriggerMode> _CleaningIntervalMode
               = new Element<EnumCleaningTriggerMode>();
        public Element<EnumCleaningTriggerMode> CleaningIntervalMode
        {
            get { return _CleaningIntervalMode; }
            set
            {
                if (value != _CleaningIntervalMode)
                {
                    _CleaningIntervalMode = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<int> _IntervalCount = new Element<int>();
        public Element<int> IntervalCount
        {
            get { return _IntervalCount; }
            set
            {
                if (value != _IntervalCount)
                {
                    _IntervalCount = value;
                    RaisePropertyChanged();
                }
            }
        }


        private Element<EnumCleaningType> _CleaningType = new Element<EnumCleaningType>();
        /// <summary>
        /// ABRAISIVE,GEL,COMBO => Cleaning 종류.
        /// </summary>
        public Element<EnumCleaningType> CleaningType
        {
            get { return _CleaningType; }
            set
            {
                if (value != _CleaningType)
                {
                    _CleaningType = value;
                    RaisePropertyChanged();
                }
            }
        }

        private PolishWaferCleaningParameter _PolishWaferCleaningInterValParam
             = new PolishWaferCleaningParameter();
        public PolishWaferCleaningParameter PolishWaferCleaningInterValParam
        {
            get { return _PolishWaferCleaningInterValParam; }
            set
            {
                if (value != _PolishWaferCleaningInterValParam)
                {
                    _PolishWaferCleaningInterValParam = value;
                    RaisePropertyChanged();
                }
            }
        }

        #endregion
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

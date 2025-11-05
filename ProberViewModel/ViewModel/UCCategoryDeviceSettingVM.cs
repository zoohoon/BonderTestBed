using System;

namespace WizardDeviceSettingViewModel
{
    using ProberErrorCode;
    using ProberInterfaces.Wizard;
    using RelayCommandBase;
    using System.ComponentModel;
    public class UCCategoryDeviceSettingVM : INotifyPropertyChanged, ICategoryDeviceSettingVM
    {

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        #region //..Properies

        private IWizardMainVM WizardMainVM { get; set; }

        #endregion

        #region //..Commands


        private RelayCommand _ChangeCategoryPadgeCommand;
        public RelayCommand ChangeCategoryPadgeCommand
        {
            get
            {
                if (null == _ChangeCategoryPadgeCommand) _ChangeCategoryPadgeCommand = new RelayCommand(ChangeCategoryPadge);
                return _ChangeCategoryPadgeCommand;
            }
        }
        private void ChangeCategoryPadge()
        {
            WizardMainVM.SetTargetWizardCategory();
        }

        private RelayCommand _SetupStartCommand;
        public RelayCommand SetupStartCommand
        {
            get
            {
                if (null == _SetupStartCommand) _SetupStartCommand = new RelayCommand(SetupStart);
                return _SetupStartCommand;
            }
        }
        private void SetupStart()
        {

        }

        #endregion


        public UCCategoryDeviceSettingVM()
        {

        }

        public UCCategoryDeviceSettingVM(IWizardMainVM wizardMainVM)
        {
            WizardMainVM = wizardMainVM;
        }

        public EventCodeEnum InitViewModel(object param)
        {


            return EventCodeEnum.NONE;
        }

    }
}

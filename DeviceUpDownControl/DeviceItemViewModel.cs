using System;

namespace DeviceUpDownControl
{
    using System.ComponentModel;

    public class DeviceItemViewModel : INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }
        #endregion

        //==> Tab의 제목, List Box의 Item 항목의 이름에 바인딩 되어 있다.
        #region ==> DeviceName
        private String _DeviceName;
        public String DeviceName
        {
            get { return _DeviceName; }
            set
            {
                if (value != _DeviceName)
                {
                    _DeviceName = value;
                    NotifyPropertyChanged("DeviceName");
                }
            }
        }
        #endregion

        private DeviceItemViewModel()
        {

        }

        public DeviceItemViewModel(String deviceName)
        {
            _DeviceName = deviceName;
        }
    }
}

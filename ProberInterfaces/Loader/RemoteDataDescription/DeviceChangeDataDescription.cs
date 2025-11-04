namespace ProberInterfaces //Loader.RemoteDataDescription
{
    using System.Collections.ObjectModel;
    using System.Runtime.Serialization;
    [DataContract]
    public class DeviceChangeDataDescription
    {
        private string _SearchStr;
        [DataMember]
        public string SearchStr
        {
            get { return _SearchStr; }
            set { _SearchStr = value; }
        }

        private bool _IsSearchDataClearButtonVisible;
        [DataMember]
        public bool IsSearchDataClearButtonVisible
        {
            get { return _IsSearchDataClearButtonVisible; }
            set { _IsSearchDataClearButtonVisible = value; }
        }

        private ObservableCollection<DeviceInfo> _ShowingDeviceInfoCollection;
        [DataMember]
        public ObservableCollection<DeviceInfo> ShowingDeviceInfoCollection
        {
            get { return _ShowingDeviceInfoCollection; }
            set { _ShowingDeviceInfoCollection = value; }
        }

        //private DeviceInfo _SelectedDeviceInfo;
        //[DataMember]
        //public DeviceInfo SelectedDeviceInfo
        //{
        //    get { return _SelectedDeviceInfo; }
        //    set { _SelectedDeviceInfo = value; }
        //}

        private DeviceInfo _ShowingDevice;
        [DataMember]
        public DeviceInfo ShowingDevice
        {
            get { return _ShowingDevice; }
            set { _ShowingDevice = value; }
        }
    }
}

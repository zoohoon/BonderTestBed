using System.Collections.ObjectModel;
using System.ComponentModel;

namespace SECS_Host.Model
{
    public class TimeOut
    {
        string name;
        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        int timeOutTime;
        public int TimeOutTime
        {
            get { return timeOutTime; }
            set { timeOutTime = value; }
        }
    }

    public class ConnectInfo : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChange(string propName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }

        private string deviceID;
        public string DeviceID
        {
            get { return deviceID; }
            set
            {
                if (deviceID != value)
                {
                    deviceID = value;
                    NotifyPropertyChange("DeviceID");
                }
            }
        }

        private string _IP;
        public string IP
        {
            get { return _IP; }
            set
            {
                if (_IP != value)
                {
                    _IP = value;
                    NotifyPropertyChange("IP");
                }
            }
        }

        private int port;
        public int Port
        {
            get { return port; }
            set
            {
                if (port != value)
                {
                    port = value;
                    NotifyPropertyChange("Port");
                }
            }
        }

        private int retryLimit;
        public int RetryLimit
        {
            get { return retryLimit; }
            set
            {
                if (retryLimit != value)
                {
                    retryLimit = value;
                    NotifyPropertyChange("RetryLimit");
                }
            }
        }

        private int linkTestInterval;
        public int LinkTestInterval
        {
            get { return linkTestInterval; }
            set
            {
                if (linkTestInterval != value)
                {
                    linkTestInterval = value;
                    NotifyPropertyChange("LinkTestInterval");
                }
            }
        }

        private string connectMode;
        public string ConnectMode
        {
            get { return connectMode; }
            set
            {
                if (connectMode != value)
                {
                    connectMode = value;
                    NotifyPropertyChange("ConnectMode");
                }
            }
        }

        private ObservableCollection<TimeOut> timeoutTimeList;
        public ObservableCollection<TimeOut> TimeoutTimeList
        {
            get { return timeoutTimeList; }
            set
            {
                if(timeoutTimeList != value)
                {
                    timeoutTimeList = value;
                    NotifyPropertyChange("TimeoutTimeList");
                }
            }
        }

        public ConnectInfo()
        {
            TimeoutTimeList = new ObservableCollection<TimeOut>();
        }
    }
}

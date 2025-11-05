using System;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace ProberInterfaces
{
    public class ProbingInfo : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String info)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));

        private ObservableCollection<IDeviceObject> _UnderDutDevs;
        public ObservableCollection<IDeviceObject> UnderDutDevs
        {
            get
            {
                return _UnderDutDevs;
            }
            set
            {
                if (_UnderDutDevs != value)
                {
                    _UnderDutDevs = value;
                    
                    NotifyPropertyChanged(nameof(UnderDutDevs));
                }
            }
        }

        private double _Progress;
        public double Progress
        {
            get
            {
                return _Progress;
            }
            set
            {
                if (_Progress != value)
                {
                    _Progress = value;
                    NotifyPropertyChanged(nameof(Progress));
                }
            }
        }


    }
}

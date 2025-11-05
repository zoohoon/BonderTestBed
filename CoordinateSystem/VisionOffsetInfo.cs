using System;
using System.ComponentModel;

namespace CoordinateSystem
{

    public class VisionOffsetInfo : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        private double _WHOffset;
        public double WHOffset
        {
            get { return _WHOffset; }
            set
            {
                if (value != _WHOffset)
                {
                    _WHOffset = value;
                    NotifyPropertyChanged("WHOffset");
                }
            }
        }
    }
}

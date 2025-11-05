using System;

namespace ProberInterfaces
{
    using System.ComponentModel;
    public class CamLightParam : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        private EnumProberCam _CamType;
        public EnumProberCam CamType
        {
            get { return _CamType; }
            set
            {
                if (value != _CamType)
                {
                    _CamType = value;
                    NotifyPropertyChanged("CamType");
                }
            }
        }



        public CamLightParam(EnumProberCam camtype)
        {
            CamType = camtype;
        }

    }
}

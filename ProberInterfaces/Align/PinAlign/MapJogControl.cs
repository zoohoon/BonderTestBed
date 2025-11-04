using LogModule;
using System;
using System.ComponentModel;

namespace ProberInterfaces.PinAlign
{
    public class MapJogControl : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }
        public MapJogControl()
        {
            try
            {
            _ZoomLevel = 4;
            _ViewCurrentPosX = 0;
            _ViewCurrentPosY = 0;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
        }
        public void ResetControl()
        {
            try
            {
            _ZoomLevel = 4;
            _ViewCurrentPosX = 0;
            _ViewCurrentPosY = 0;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
        }
        private double _ZoomLevel;
        public double ZoomLevel
        {
            get { return _ZoomLevel; }
            set { _ZoomLevel = value; }
        }
        private double _ViewCurrentPosX;
        public double ViewCurrentPosX
        {
            get { return _ViewCurrentPosX; }
            set
            {
                _ViewCurrentPosX = value;
            }

        }

        private double _ViewCurrentPosY;
        public double ViewCurrentPosY
        {
            get { return _ViewCurrentPosY; }
            set
            {
                _ViewCurrentPosY = value;
            }
        }
    }
}

using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace PMIManualResultVM
{
    public struct PMIResultSummary : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged([CallerMemberName]string propName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));

        private double _AvgMarkPosX;
        public double AvgMarkPosX
        {
            get { return _AvgMarkPosX; }
            set
            {
                if (value != _AvgMarkPosX)
                {
                    _AvgMarkPosX = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _AvgMarkPosY;
        public double AvgMarkPosY
        {
            get { return _AvgMarkPosY; }
            set
            {
                if (value != _AvgMarkPosY)
                {
                    _AvgMarkPosY = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _AvgMarkSizeX;
        public double AvgMarkSizeX
        {
            get { return _AvgMarkSizeX; }
            set
            {
                if (value != _AvgMarkSizeX)
                {
                    _AvgMarkSizeX = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _AvgMarkSizeY;
        public double AvgMarkSizeY
        {
            get { return _AvgMarkSizeY; }
            set
            {
                if (value != _AvgMarkSizeY)
                {
                    _AvgMarkSizeY = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _AvgMarkArea;
        public double AvgMarkArea
        {
            get { return _AvgMarkArea; }
            set
            {
                if (value != _AvgMarkArea)
                {
                    _AvgMarkArea = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _MarkMinArea;
        public double MarkMinArea
        {
            get { return _MarkMinArea; }
            set
            {
                if (value != _MarkMinArea)
                {
                    _MarkMinArea = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _MarkMaxArea;
        public double MarkMaxArea
        {
            get { return _MarkMaxArea; }
            set
            {
                if (value != _MarkMaxArea)
                {
                    _MarkMaxArea = value;
                    RaisePropertyChanged();
                }
            }
        }

        public static bool operator ==(PMIResultSummary source, PMIResultSummary dest)
        {
            bool retVal = false;
            try
            {

            retVal =
                source.AvgMarkPosX == dest.AvgMarkPosX
                && source.AvgMarkPosY == dest.AvgMarkPosY
                && source.AvgMarkSizeX == dest.AvgMarkSizeX
                && source.AvgMarkSizeY == dest.AvgMarkSizeY
                && source.AvgMarkArea == dest.AvgMarkArea
                && source.MarkMinArea == dest.MarkMinArea
                && source.MarkMaxArea == dest.MarkMaxArea;

            }
            catch (Exception err)
            {
                
                 throw;
            }
            return retVal;
        }


        public static bool operator !=(PMIResultSummary source, PMIResultSummary dest)
        {
            bool retVal = false;

            retVal =
                source.AvgMarkPosX == dest.AvgMarkPosX
                && source.AvgMarkPosY == dest.AvgMarkPosY
                && source.AvgMarkSizeX == dest.AvgMarkSizeX
                && source.AvgMarkSizeY == dest.AvgMarkSizeY
                && source.AvgMarkArea == dest.AvgMarkArea
                && source.MarkMinArea == dest.MarkMinArea
                && source.MarkMaxArea == dest.MarkMaxArea;

            return !retVal;
        }

    }
}

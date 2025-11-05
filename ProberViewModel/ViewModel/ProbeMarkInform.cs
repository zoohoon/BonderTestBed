using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace PMIManualResultVM
{
    public struct ProbeMarkInform : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged([CallerMemberName]string propName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));

        private double _AreaSum;
        public double AreaSum
        {
            get { return _AreaSum; }
            set
            {
                if (value != _AreaSum)
                {
                    _AreaSum = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _SizeXSum;
        public double SizeXSum
        {
            get { return _SizeXSum; }
            set
            {
                if (value != _SizeXSum)
                {
                    _SizeXSum = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _SizeYSum;
        public double SizeYSum
        {
            get { return _SizeYSum; }
            set
            {
                if (value != _SizeYSum)
                {
                    _SizeYSum = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _PositionX;
        public double PositionX
        {
            get { return _PositionX; }
            set
            {
                if (value != _PositionX)
                {
                    _PositionX = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _PositionY;
        public double PositionY
        {
            get { return _PositionY; }
            set
            {
                if (value != _PositionY)
                {
                    _PositionY = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string _FailCode;
        public string FailCode
        {
            get { return _FailCode; }
            set
            {
                if (value != _FailCode)
                {
                    _FailCode = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _SizeX;
        public double SizeX
        {
            get { return _SizeX; }
            set
            {
                if (value != _SizeX)
                {
                    _SizeX = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _SizeY;
        public double SizeY
        {
            get { return _SizeY; }
            set
            {
                if (value != _SizeY)
                {
                    _SizeY = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _Area01;
        public double Area01
        {
            get { return _Area01; }
            set
            {
                if (value != _Area01)
                {
                    _Area01 = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _Area02;
        public double Area02
        {
            get { return _Area02; }
            set
            {
                if (value != _Area02)
                {
                    _Area02 = value;
                    RaisePropertyChanged();
                }
            }
        }

        public static bool operator ==(ProbeMarkInform source, ProbeMarkInform dest)
        {
            bool retVal = false;
            try
            {

                retVal =
                    source.AreaSum == dest.AreaSum
                    && source.SizeXSum == dest.SizeXSum
                    && source.SizeYSum == dest.SizeYSum
                    && source.PositionX == dest.PositionX
                    && source.PositionY == dest.PositionY
                    && source.FailCode == dest.FailCode
                    && source.SizeX == dest.SizeX
                    && source.SizeY == dest.SizeY
                    && source.Area01 == dest.Area01
                    && source.Area02 == dest.Area02;

            }
            catch (Exception err)
            {
                throw;
            }
            return retVal;
        }

        public static bool operator !=(ProbeMarkInform source, ProbeMarkInform dest)
        {
            bool retVal = false;

            try
            {
                retVal =
                        source.AreaSum == dest.AreaSum
                        && source.SizeXSum == dest.SizeXSum
                        && source.SizeYSum == dest.SizeYSum
                        && source.PositionX == dest.PositionX
                        && source.PositionY == dest.PositionY
                        && source.FailCode == dest.FailCode
                        && source.SizeX == dest.SizeX
                        && source.SizeY == dest.SizeY
                        && source.Area01 == dest.Area01
                        && source.Area02 == dest.Area02;
            }
            catch (Exception err)
            {
                throw;
            }

            return !retVal;
        }
    }
}

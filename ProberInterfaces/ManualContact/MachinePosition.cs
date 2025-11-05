using System;

namespace ProberInterfaces
{
    using LogModule;
    using System.ComponentModel;
    public class MachinePosition : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged(string propName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }

        public MachinePosition()
        {
        }

        public MachinePosition(double x, double y, double z, double t) : base()
        {
            try
            {
                this.X = x;
                this.Y = y;
                this.Z = z;
                this.T = t;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public void ChangePosition(double x, double y, double z, double t)
        {
            try
            {
                this.X = x;
                this.Y = y;
                this.Z = z;
                this.T = t;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        private double _X;
        public double X
        {
            get { return _X; }
            set
            {
                if (value != _X)
                {
                    _X = value;
                    RaisePropertyChanged(nameof(X));
                }
            }
        }

        private double _Y;
        public double Y
        {
            get { return _Y; }
            set
            {
                if (value != _Y)
                {
                    _Y = value;
                    RaisePropertyChanged(nameof(Y));
                }
            }
        }

        private double _Z;
        public double Z
        {
            get { return _Z; }
            set
            {
                if (value != _Z)
                {
                    _Z = value;
                    RaisePropertyChanged(nameof(Z));
                }
            }
        }

        private double _T;
        public double T
        {
            get { return _T; }
            set
            {
                if (value != _T)
                {
                    _T = value;
                    RaisePropertyChanged(nameof(T));
                }
            }
        }
    }
}

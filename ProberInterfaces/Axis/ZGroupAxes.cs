using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ProberInterfaces
{
    public class ZGroupAxes : INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        private double _Z0Pos;
        public double Z0Pos
        {
            get { return _Z0Pos; }
            set
            {
                if (value != _Z0Pos)
                {
                    _Z0Pos = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _Z1Pos;
        public double Z1Pos
        {
            get { return _Z1Pos; }
            set
            {
                if (value != _Z1Pos)
                {
                    _Z1Pos = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _Z2Pos;
        public double Z2Pos
        {
            get { return _Z2Pos; }
            set
            {
                if (value != _Z2Pos)
                {
                    _Z2Pos = value;
                    RaisePropertyChanged();
                }
            }
        }
    }
}

using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace ExtraCameraDialogVM
{
    public class LineClass : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged([CallerMemberName]string propName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));

        private Point _StartPoint;
        public Point StartPoint
        {
            get { return _StartPoint; }
            set
            {
                if (value != _StartPoint)
                {
                    _StartPoint = value;
                    RaisePropertyChanged(nameof(StartPoint));
                }
            }
        }

        private Point _EndPoint;
        public Point EndPoint
        {
            get { return _EndPoint; }
            set
            {
                if (value != _EndPoint)
                {
                    _EndPoint = value;
                    RaisePropertyChanged(nameof(EndPoint));
                }
            }
        }
    }
}

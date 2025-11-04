using ProberInterfaces.PolishWafer;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace PolishWaferModule
{
    public class PolishWaferControlItems : IPolishWaferControlItems, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        private bool _POLISHWAFER_CENTERING_ERROR;
        public bool POLISHWAFER_CENTERING_ERROR
        {
            get { return _POLISHWAFER_CENTERING_ERROR; }
            set
            {
                if (value != _POLISHWAFER_CENTERING_ERROR)
                {
                    _POLISHWAFER_CENTERING_ERROR = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _POLISHWAFER_FOCUSING_ERROR;
        public bool POLISHWAFER_FOCUSING_ERROR
        {
            get { return _POLISHWAFER_FOCUSING_ERROR; }
            set
            {
                if (value != _POLISHWAFER_FOCUSING_ERROR)
                {
                    _POLISHWAFER_FOCUSING_ERROR = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _POLISHWAFER_CLEAING_ERROR;
        public bool POLISHWAFER_CLEAING_ERROR
        {
            get { return _POLISHWAFER_CLEAING_ERROR; }
            set
            {
                if (value != _POLISHWAFER_CLEAING_ERROR)
                {
                    _POLISHWAFER_CLEAING_ERROR = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _POLISHWAFER_CLEANING_MARGIN_EXCEEDED;
        public bool POLISHWAFER_CLEANING_MARGIN_EXCEEDED
        {
            get { return _POLISHWAFER_CLEANING_MARGIN_EXCEEDED; }
            set
            {
                if (value != _POLISHWAFER_CLEANING_MARGIN_EXCEEDED)
                {
                    _POLISHWAFER_CLEANING_MARGIN_EXCEEDED = value;
                    RaisePropertyChanged();
                }
            }
        }
    }
}

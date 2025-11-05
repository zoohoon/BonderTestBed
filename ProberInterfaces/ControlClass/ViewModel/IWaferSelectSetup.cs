using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace ProberInterfaces
{
    public interface IWaferSelectSetup
    {
        ObservableCollection<ButtonDescriptor> WaferSelectBtn { get; set; }
        ButtonDescriptor SelectAllBtn { get; set; }
        ButtonDescriptor ClearAllBtn { get; set; }
    }

    public class ButtonDescriptor : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged([CallerMemberName]string propName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }

        public ButtonDescriptor()
        {

        }

        private ICommand _Command = null;
        public ICommand Command
        {
            get { return _Command; }
            set
            {
                if (value != _Command)
                {
                    _Command = value;
                    RaisePropertyChanged();
                }
            }
        }

        private object _CommandParameter = null;
        public object CommandParameter
        {
            get { return _CommandParameter; }
            set
            {
                if (value != _CommandParameter)
                {
                    _CommandParameter = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _isChecked = false;
        public bool isChecked
        {
            get { return _isChecked; }
            set
            {
                if (value != _isChecked)
                {
                    _isChecked = value;
                    RaisePropertyChanged();
                }
            }
        }

    }

}

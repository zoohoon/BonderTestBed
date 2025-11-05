using ProberInterfaces;
using RelayCommandBase;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;

namespace SorterSystem.VM
{
    public class IOControlVM : INotifyPropertyChanged, IFactoryModule
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        private InputChannel _Input;
        public InputChannel Input
        {
            get { return _Input; }
            set
            {
                if (_Input != value)
                {
                    _Input = value;
                    RaisePropertyChanged();
                }
            }
        }

        private OutputChannel _Output;
        public OutputChannel Output
        {
            get { return _Output; }
            set
            {
                if (_Output != value)
                {
                    _Output = value;
                    RaisePropertyChanged();
                }
            }
        }

        private RelayCommand<object> _DoubleClickCommand;
        public RelayCommand<object> DoubleClickCommand
        {
            get
            {
                if (_DoubleClickCommand == null)
                    _DoubleClickCommand = new RelayCommand<object>(OnMouseDoubleClicked);
                return _DoubleClickCommand;
            }
        }


        private void OnMouseDoubleClicked(object obj)
        {
            IOPortDescripter<bool> port = obj as IOPortDescripter<bool>;
            if (port != null) if (port.Value) port.ResetValue(); else port.SetValue();
        }


        private ObservableCollection<IOPortDescripter<bool>> _InputPorts = new ObservableCollection<IOPortDescripter<bool>>();
        public ObservableCollection<IOPortDescripter<bool>> InputPorts
        {
            get { return _InputPorts; }
            set
            {
                if (value != _InputPorts)
                {
                    _InputPorts = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ObservableCollection<IOPortDescripter<bool>> _outputPorts = new ObservableCollection<IOPortDescripter<bool>>();
        public ObservableCollection<IOPortDescripter<bool>> OutputPorts
        {
            get { return _outputPorts; }
            set
            {
                if (value != _outputPorts)
                {
                    _outputPorts = value;
                    RaisePropertyChanged();
                }
            }
        }

        private AnalogInputChannel _AnalogInput;
        public AnalogInputChannel AnalogInput
        {
            get { return _AnalogInput; }
            set
            {
                if (_AnalogInput != value)
                {
                    _AnalogInput = value;
                    RaisePropertyChanged();
                }
            }
        }

        public class AnalogPortDescripter : IOPortDescripter<long>
        {
            public AnalogPortDescripter(int _chan, int _port, string _key, EnumIOType _type, PortValue val)
                : base(_chan, _port, _key, _type)
            {
                _value = val;
            }
            private PortValue _value;
            public PortValue Analog
            {
                get
                {
                    return _value;
                }
            }
        }

        private ObservableCollection<AnalogPortDescripter> _AnalogInputs = new ObservableCollection<AnalogPortDescripter>();
        public ObservableCollection<AnalogPortDescripter> AnalogInputs
        {
            get { return _AnalogInputs; }
            set
            {
                if (value != _AnalogInputs)
                {
                    _AnalogInputs = value;
                    RaisePropertyChanged();
                }
            }
        }
    }
}

using LogModule;
using ProberInterfaces;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace ForcedIODialog
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ForcedIOView : Window, INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        private static ForcedIOView forcedIOView;

        public static ForcedIOView GetInstance()
        {
            if (forcedIOView == null)
            {
                forcedIOView = new ForcedIOView();
            }
            return forcedIOView;
        }

        private IOPortDescripter<bool> _DIWAFERONARM;
        public IOPortDescripter<bool> DIWAFERONARM
        {
            get { return _DIWAFERONARM; }
            set
            {
                if (value != _DIWAFERONARM)
                {
                    _DIWAFERONARM = value;
                    RaisePropertyChanged();
                }
            }
        }

        private IOPortDescripter<bool> _DIWAFERONARM2;
        public IOPortDescripter<bool> DIWAFERONARM2
        {
            get { return _DIWAFERONARM2; }
            set
            {
                if (value != _DIWAFERONARM2)
                {
                    _DIWAFERONARM2 = value;
                    RaisePropertyChanged();
                }
            }
        }

        private IOPortDescripter<bool> _DIWAFERONSUBCHUCK;
        public IOPortDescripter<bool> DIWAFERONSUBCHUCK
        {
            get { return _DIWAFERONSUBCHUCK; }
            set
            {
                if (value != _DIWAFERONSUBCHUCK)
                {
                    _DIWAFERONSUBCHUCK = value;
                    RaisePropertyChanged();
                }
            }
        }
        private IOPortDescripter<bool> _DIWAFERONDRAWER;
        public IOPortDescripter<bool> DIWAFERONDRAWER
        {
            get { return _DIWAFERONDRAWER; }
            set
            {
                if (value != _DIWAFERONDRAWER)
                {
                    _DIWAFERONDRAWER = value;
                    RaisePropertyChanged();
                }
            }
        }
        private IOPortDescripter<bool> _DIWAFERONFIXEDTRAY0;
        public IOPortDescripter<bool> DIWAFERONFIXEDTRAY0
        {
            get { return _DIWAFERONFIXEDTRAY0; }
            set
            {
                if (value != _DIWAFERONFIXEDTRAY0)
                {
                    _DIWAFERONFIXEDTRAY0 = value;
                    RaisePropertyChanged();
                }
            }
        }
        private IOPortDescripter<bool> _DIWAFERONFIXEDTRAY1;
        public IOPortDescripter<bool> DIWAFERONFIXEDTRAY1
        {
            get { return _DIWAFERONFIXEDTRAY1; }
            set
            {
                if (value != _DIWAFERONFIXEDTRAY1)
                {
                    _DIWAFERONFIXEDTRAY1 = value;
                    RaisePropertyChanged();
                }
            }
        }
        private IOPortDescripter<bool> _DIWAFERONFIXEDTRAY2;
        public IOPortDescripter<bool> DIWAFERONFIXEDTRAY2
        {
            get { return _DIWAFERONFIXEDTRAY2; }
            set
            {
                if (value != _DIWAFERONFIXEDTRAY2)
                {
                    _DIWAFERONFIXEDTRAY2 = value;
                    RaisePropertyChanged();
                }
            }
        }
        private IOPortDescripter<bool> _DIWAFERONFIXEDTRAY3;
        public IOPortDescripter<bool> DIWAFERONFIXEDTRAY3
        {
            get { return _DIWAFERONFIXEDTRAY3; }
            set
            {
                if (value != _DIWAFERONFIXEDTRAY3)
                {
                    _DIWAFERONFIXEDTRAY3 = value;
                    RaisePropertyChanged();
                }
            }
        }
        private IOPortDescripter<bool> _DIWAFERONFIXEDTRAY4;
        public IOPortDescripter<bool> DIWAFERONFIXEDTRAY4
        {
            get { return _DIWAFERONFIXEDTRAY4; }
            set
            {
                if (value != _DIWAFERONFIXEDTRAY4)
                {
                    _DIWAFERONFIXEDTRAY4 = value;
                    RaisePropertyChanged();
                }
            }
        }

        private IOPortDescripter<bool> _DIWAFERONCHUCK_6;
        public IOPortDescripter<bool> DIWAFERONCHUCK_6
        {
            get { return _DIWAFERONCHUCK_6; }
            set
            {
                if (value != _DIWAFERONCHUCK_6)
                {
                    _DIWAFERONCHUCK_6 = value;
                    RaisePropertyChanged();
                }
            }
        }
        private IOPortDescripter<bool> _DIWAFERONCHUCK_8;
        public IOPortDescripter<bool> DIWAFERONCHUCK_8
        {
            get { return _DIWAFERONCHUCK_8; }
            set
            {
                if (value != _DIWAFERONCHUCK_8)
                {
                    _DIWAFERONCHUCK_8 = value;
                    RaisePropertyChanged();
                }
            }
        }
        private IOPortDescripter<bool> _DIWAFERONCHUCK_12;
        public IOPortDescripter<bool> DIWAFERONCHUCK_12
        {
            get { return _DIWAFERONCHUCK_12; }
            set
            {
                if (value != _DIWAFERONCHUCK_12)
                {
                    _DIWAFERONCHUCK_12 = value;
                    RaisePropertyChanged();
                }
            }
        }
        private IOPortDescripter<bool> _DITHREELEGUP_0;
        public IOPortDescripter<bool> DITHREELEGUP_0
        {
            get { return _DITHREELEGUP_0; }
            set
            {
                if (value != _DITHREELEGUP_0)
                {
                    _DITHREELEGUP_0 = value;
                    RaisePropertyChanged();
                }
            }
        }
        private IOPortDescripter<bool> _DITHREELEGUP_1;
        public IOPortDescripter<bool> DITHREELEGUP_1
        {
            get { return _DITHREELEGUP_1; }
            set
            {
                if (value != _DITHREELEGUP_1)
                {
                    _DITHREELEGUP_1 = value;
                    RaisePropertyChanged();
                }
            }
        }



        protected IIOManager IOManager { get; private set; }

        public ForcedIOView()
        {
            try
            {
                InitializeComponent();
                IOManager = Extensions_IModule.IOManager(null);
                DIWAFERONARM = IOManager.IO.Inputs.DIWAFERONARM;
                DIWAFERONARM2 = IOManager.IO.Inputs.DIWAFERONARM2;
                DIWAFERONSUBCHUCK = IOManager.IO.Inputs.DIWAFERONSUBCHUCK;
                DIWAFERONDRAWER = IOManager.IO.Inputs.DIWAFERONDRAWER;
                DIWAFERONFIXEDTRAY0 = IOManager.IO.Inputs.DIWAFERONFIXEDTRAY0;
                DIWAFERONFIXEDTRAY1 = IOManager.IO.Inputs.DIWAFERONFIXEDTRAY1;
                DIWAFERONFIXEDTRAY2 = IOManager.IO.Inputs.DIWAFERONFIXEDTRAY2;
                DIWAFERONFIXEDTRAY3 = IOManager.IO.Inputs.DIWAFERONFIXEDTRAY3;
                DIWAFERONFIXEDTRAY4 = IOManager.IO.Inputs.DIWAFERONFIXEDTRAY4;
                DIWAFERONCHUCK_6 = IOManager.IO.Inputs.DIWAFERONCHUCK_6;
                DIWAFERONCHUCK_8 = IOManager.IO.Inputs.DIWAFERONCHUCK_8;
                DIWAFERONCHUCK_12 = IOManager.IO.Inputs.DIWAFERONCHUCK_12;
                DITHREELEGUP_0 = IOManager.IO.Inputs.DITHREELEGUP_0;
                DITHREELEGUP_1 = IOManager.IO.Inputs.DITHREELEGUP_1;
                DataContext = this;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                e.Cancel = true;
                this.Visibility = Visibility.Hidden;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

    }
}

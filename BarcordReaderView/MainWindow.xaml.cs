using BCDCLV50x;
using LogModule;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Timers;
using System.Windows;

namespace BarcordReaderView
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
        public MainWindow()
        {
            InitializeComponent();
        }
        Timer timer = new Timer();
        private CLVBCDSensor BacordReader { get; set; }
        public MainWindow(CLVBCDSensor bacordReader) : this()
        {
            try
            {
                BacordReader = bacordReader;
                DataContext = this;
                timer = new System.Timers.Timer(1000);
                timer.Elapsed += OnTimedEvent;
                timer.AutoReset = true;
                timer.Enabled = true;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        private String _BacordString;
        public String BacordString
        {
            get { return _BacordString; }
            set
            {
                if (value != _BacordString)
                {
                    _BacordString = value;
                    RaisePropertyChanged();
                }
            }
        }
        private void OnTimedEvent(Object source, ElapsedEventArgs e)
        {
            if(BacordReader!=null)
            {
                if (BacordReader.DataReady == true)
                {
                    BacordString = BacordReader.ReceivedBCD;
                    LoggerManager.Debug($"PC ID: {BacordReader.ReceivedBCD}");
                    BacordReader.Clear();
                }
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            timer.Stop();
            timer.Dispose();
        }

      

        private void ClearBtn_Click(object sender, RoutedEventArgs e)
        {
            BacordString = "";
        }
    }
}

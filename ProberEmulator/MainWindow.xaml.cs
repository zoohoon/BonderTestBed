using MahApps.Metro.Controls;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows;
using LogModule;

namespace ProberEmulator
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : MetroWindow, INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        private string _Version;
        public string Version
        {
            get { return _Version; }
            set
            {
                if (value != _Version)
                {
                    _Version = value;
                    RaisePropertyChanged();
                }
            }
        }

        public MainWindow()
        {
            InitializeComponent();

            DataContext = this;

            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(System.Reflection.Assembly.GetEntryAssembly().Location);
            //LoggerManager.Debug($"Get FileVersion.");
            Version = fvi.FileVersion;
        }

        private void MetroWindow_Loaded(object sender, RoutedEventArgs e)
        {
            ViewProberEmulator.Loaded();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    System.Windows.Application.Current.Shutdown();
                    Environment.Exit(0);
                });
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
            }
        }
    }
}

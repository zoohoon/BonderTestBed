using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using LogModule;
using MahApps.Metro.Controls;

namespace SystemPult
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow, INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
        PultViewModel viewModel;
        public MainWindow()
        {
            LoggerManager.Init();

            viewModel = new PultViewModel();
            viewModel.SetContainer();

            this.DataContext = viewModel;
            //viewModel.InitModule();

            InitializeComponent();
            
        }

        private void MetroWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //EventCodeEnum retval = EventCodeEnum.UNDEFINED;
            try
            {
                viewModel.DeInitModule();

                App.Current.Shutdown();
            }
            catch (Exception err)
            {
                LoggerManager.Error($"MetroWindow_Closing(): Error occurred. Err = {err.Message}");
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}

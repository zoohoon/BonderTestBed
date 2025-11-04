using System;
using System.Windows;

namespace EventCodeEditor
{
    using LoaderMaster;
    using LogModule;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
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

        public MainWindow(LoaderSupervisor master) : this()
        {
            try
            {
                DataContext = this;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }



    }
}

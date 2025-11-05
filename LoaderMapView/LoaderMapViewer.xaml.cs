using MahApps.Metro.Controls;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace LoaderMapView
{
    /// <summary>
    /// LoaderMapViewer.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class LoaderMapViewer : MetroWindow, INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler PropertyChanged;
        LoaderMapViewModel vm = LoaderMapViewModel.Instance;
        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        public LoaderMapViewer()
        {
            InitializeComponent();
            this.DataContext = vm;
        }


    }
}

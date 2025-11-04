using System.Windows;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ProberViewModel
{
    /// <summary>
    /// GPFoupSettingView.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class GPLoaderJobWindow : Window, INotifyPropertyChanged
    {

        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
        public GPLoaderJobWindow()
        {
            InitializeComponent();
        }
    }
}

using ProberInterfaces.Foup;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Controls;


namespace FoupMainControl
{
    /// <summary>
    /// Foup8Inch.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Foup8Inch : UserControl, IFoup3DModel, INotifyPropertyChanged
    {
        public Foup8Inch()
        {
            InitializeComponent();
        }
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        public void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}

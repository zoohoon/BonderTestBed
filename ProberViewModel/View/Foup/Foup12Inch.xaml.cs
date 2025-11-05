namespace FoupMainControl
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;
    using System.Windows.Documents;
    using System.Windows.Input;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using System.Windows.Navigation;
    using System.Windows.Shapes;
    using ProberInterfaces.Foup;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Windows.Controls;

    /// <summary>
    /// Foup12Inch.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Foup12Inch : UserControl, IFoup3DModel, INotifyPropertyChanged
    {
        public Foup12Inch()
        {
            InitializeComponent();
        }
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        public void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

       
        
    }
}

using ProberInterfaces.Foup;
using System.Windows;
using System.Windows.Controls;

namespace FoupManualControl
{
    /// <summary>
    /// UserControl1.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class FoupManualControlView : UserControl, IFoupSubView
    {
        public FoupManualControlView()
        {
            InitializeComponent();
        }

        private void ToggleButton_Unchecked(object sender, RoutedEventArgs e)
        {

        }
    }
}

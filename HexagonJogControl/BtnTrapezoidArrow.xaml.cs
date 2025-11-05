using System.Windows.Controls;
using System.Windows.Input;

namespace HexagonJogControl
{
    /// <summary>
    /// Interaction logic for BtnTrapezoidArrow.xaml
    /// </summary>
    public partial class BtnTrapezoidArrow : UserControl
    {
        public BtnTrapezoidArrow()
        {
            InitializeComponent();
        }

        private void mainGrid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            mainGrid.Opacity = 0.5;
        }

        private void mainGrid_MouseUp(object sender, MouseButtonEventArgs e)
        {
            mainGrid.Opacity = 1;
        }

        private void mainGrid_MouseLeave(object sender, MouseEventArgs e)
        {
            mainGrid.Opacity = 1;
        }
    }
}

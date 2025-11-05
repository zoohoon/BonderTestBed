using System.Windows.Controls;
using System.Windows.Input;

namespace LoaderTestMainPageView.WaferHandlingUC
{
    /// <summary>
    /// UcWaferHandleIcon.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class UcWaferHandleIcon : UserControl
    {
        public UcWaferHandleIcon()
        {
            InitializeComponent();
        }

        private void icon_MouseDown(object sender, MouseButtonEventArgs e)
        {
            icon.Opacity = 0.5;
        }

        private void icon_MouseUp(object sender, MouseButtonEventArgs e)
        {
            icon.Opacity = 1;
        }

        private void icon_MouseLeave(object sender, MouseEventArgs e)
        {
            icon.Opacity = 1;
        }
    }
}

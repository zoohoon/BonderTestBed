using System;
using System.Windows.Controls;
using System.Windows.Input;

namespace TestSetupDialog
{
    using ProberInterfaces;
    /// <summary>
    /// UcTestSetupPage.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class UcTestSetupPage : UserControl, IMainScreenView
    {
        readonly Guid _ViewGUID = new Guid("0958E509-2985-42EF-857C-660E2F05789A");
        public Guid ScreenGUID { get { return _ViewGUID; } }

        public UcTestSetupPage()
        {
            InitializeComponent();
        }

        private void exitBtn_MouseDown(object sender, MouseButtonEventArgs e)
        {
            exitBtn.Opacity = 0.5;
        }

        private void exitBtn_MouseUp(object sender, MouseButtonEventArgs e)
        {
            exitBtn.Opacity = 1;
        }

        private void exitBtn_MouseLeave(object sender, MouseEventArgs e)
        {
            exitBtn.Opacity = 1;
        }
    }
}

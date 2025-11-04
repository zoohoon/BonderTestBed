using System;

namespace ProberViewModel
{
    public partial class MainMenu : System.Windows.Controls.UserControl
    {
        readonly Guid _ViewGUID = new Guid("0f9f27b8-8547-43de-87a9-21823f790b79");
        public Guid ScreenGUID { get { return _ViewGUID; } }

        public MainMenu()
        {
            InitializeComponent();
        }
    }
}

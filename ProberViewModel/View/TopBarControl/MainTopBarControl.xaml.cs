using System;
using System.Windows.Controls;
using System.Windows.Input;

namespace MainTopBarView
{
    using ProberInterfaces;

    /// <summary>
    /// Interaction logic for MainTopBarControl.xaml
    /// </summary>
    //public partial class MainTopBarControl : UserControl , IMainTopBarView
    public partial class MainTopBarControl : UserControl, IMainScreenView
    {
        readonly Guid _ViewGUID = new Guid("EC8FB988-222F-1E88-2C18-6DF6A742B3E9");
        public Guid ScreenGUID { get { return _ViewGUID; } }

        public MainTopBarControl()
        {
            InitializeComponent();
        }

        private void SimpleLogView_MouseDown(object sender, MouseButtonEventArgs e)
        {

        }
    }
}

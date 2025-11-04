using ProberInterfaces;
using System;
using System.Windows.Controls;

namespace TCPIP
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class TCPIPSettingView : UserControl, IMainScreenView
    {
        public TCPIPSettingView()
        {
            InitializeComponent();
        }

        private readonly Guid _ViewGUID = new Guid("58e07f0c-0ab5-4cc2-b054-0bccf49700d8");
        public Guid ScreenGUID
        {
            get { return _ViewGUID; }
        }
    }
}

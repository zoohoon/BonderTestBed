using ProberInterfaces;
using System;
using System.Windows.Controls;

namespace LoginControl
{
    /// <summary>
    /// UserControl1.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class LoginControl : UserControl, IMainScreenView
    {
        public LoginControl()
        {
            InitializeComponent();
        }

        readonly Guid _ViewGUID = new Guid("28A11F12-8918-47FE-8161-3652F2EFEF29");
        public Guid ScreenGUID { get { return _ViewGUID; } }
    }
}

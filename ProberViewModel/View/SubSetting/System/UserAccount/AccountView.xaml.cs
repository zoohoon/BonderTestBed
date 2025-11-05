using System;
using System.Windows.Controls;

namespace UcAccountView
{
    using ProberInterfaces;

    public partial class AccountView : UserControl, IMainScreenView
    {
        private readonly Guid _ViewGUID = new Guid("67a5e6b4-c986-4323-b05c-9139e703a0f9");
        public Guid ScreenGUID
        {
            get { return _ViewGUID; }
        }

        public AccountView()
        {
            InitializeComponent();
        }
    }
}

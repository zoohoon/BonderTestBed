using ProberInterfaces;
using System;
using System.Windows.Controls;

namespace ProberViewModel
{
    public partial class PopupPanel : UserControl, IMainScreenView
    {
        readonly Guid _ViewGUID = new Guid("A0FD12AD-D621-4312-84C9-D54C9018FA11");
        public Guid ScreenGUID { get { return _ViewGUID; } }

        public PopupPanel()
        {
            InitializeComponent();
        }
    }
}

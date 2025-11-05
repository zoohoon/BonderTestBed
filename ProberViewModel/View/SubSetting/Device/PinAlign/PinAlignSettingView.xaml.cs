using ProberInterfaces;
using System;
using System.Windows.Controls;

namespace UcPinAlignSettingView
{
    public partial class PinAlignSettingView : UserControl, IMainScreenView
    {
        public PinAlignSettingView()
        {
            InitializeComponent();
        }

        private Guid _ViewGUID = new Guid("24389CE4-FA92-44B7-9D31-FDBFE2CAB2DF");
        public Guid ScreenGUID { get { return _ViewGUID; } }

    }
}

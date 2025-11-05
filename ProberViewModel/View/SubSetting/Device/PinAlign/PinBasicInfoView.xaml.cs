using ProberInterfaces;
using System;
using System.Windows.Controls;

namespace UcPinBasicInfoView
{
    public partial class PinBasicInfoView : UserControl, IMainScreenView
    {
        public PinBasicInfoView()
        {
            InitializeComponent();
        }

        private readonly Guid _ViewGUID = new Guid("edf7abaa-b134-40bf-b970-0dbf0f8afa45");
        public Guid ScreenGUID
        {
            get { return _ViewGUID; }
        }
    }
}

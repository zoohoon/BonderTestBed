using ProberInterfaces;
using System;
using System.Windows.Controls;

namespace UcGpibSettingView
{
    public partial class GpibSettingView : UserControl, IMainScreenView
    {
        public GpibSettingView()
        {
            InitializeComponent();
        }

        private readonly Guid _ViewGUID = new Guid("5D647D3B-AFFD-4A4D-9C1F-D0AD8C3D1A75");
        public Guid ScreenGUID
        {
            get { return _ViewGUID; }
        }
    }
}

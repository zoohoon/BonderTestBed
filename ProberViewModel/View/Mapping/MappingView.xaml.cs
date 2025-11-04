using ProberInterfaces;
using System;
using System.Windows.Controls;

namespace UcMappingView
{
    public partial class MappingView : UserControl, IMainScreenView
    {
        public MappingView()
        {
            InitializeComponent();
        }

        private Guid _ViewGUID = new Guid("5E911839-A446-4C6C-BED4-A7EFDA752BFD");
        public Guid ScreenGUID { get => _ViewGUID; }
    }
}

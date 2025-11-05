using System;
using System.Windows.Controls;

namespace PadSettingView_Standard
{
    using ProberInterfaces;
    /// <summary>
    /// Interaction logic for PadSettingView.xaml
    /// </summary>
    public partial class PadSettingView : UserControl, IMainScreenView
    {

        private readonly Guid _ViewGUID = new Guid("56AF1E26-1C9F-1FDD-B38B-EA0C7324311F");
        public Guid ScreenGUID
        {
            get { return _ViewGUID; }
        }
        public PadSettingView()
        {
            InitializeComponent();
        }

    }
}

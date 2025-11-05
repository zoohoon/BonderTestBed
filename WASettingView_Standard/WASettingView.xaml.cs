using System;
using System.Windows.Controls;

namespace WASettingView_Standard
{
    using ProberInterfaces;
    /// <summary>
    /// Interaction logic for WASettingView.xaml
    /// </summary>
    public partial class WASettingView : UserControl, IMainScreenView
    {
        public WASettingView()
        {
            InitializeComponent();
        }

        private readonly Guid _ViewGUID = new Guid("DE32A33D-AC5D-9A21-CFDA-02A032611C11");
        public Guid ScreenGUID
        {
            get { return _ViewGUID; }
        }
    }
}

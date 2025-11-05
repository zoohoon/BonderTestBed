using ProberInterfaces;
using System;
using System.Windows.Controls;

namespace SoakingSettingView
{
    /// <summary>
    /// UCSoaking.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ManualSoakingView : UserControl,IMainScreenView
    {
        public ManualSoakingView()
        {
            InitializeComponent();
        }
        readonly Guid _ViewGUID = new Guid("8B2993EA-7358-43CD-91BC-BAD430C0A9F4");
        public Guid ScreenGUID { get { return _ViewGUID; } }
    }
}

using ProberInterfaces;
using System;
using System.Windows.Controls;

namespace SoakingSettingView
{
    /// <summary>
    /// UCSoaking.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class UCSoaking : UserControl,IMainScreenView
    {
        public UCSoaking()
        {
            InitializeComponent();
        }
        readonly Guid _ViewGUID = new Guid("DEC778CB-3012-5DF2-3B1D-A90129B1C417");
        public Guid ScreenGUID { get { return _ViewGUID; } }
    }
}

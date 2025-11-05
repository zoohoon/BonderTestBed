using System;
using System.Windows.Controls;
using ProberInterfaces;

namespace MaskingSettingControl
{
    /// <summary>
    /// UserControl1.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MaskingSettingControl : UserControl, IMainScreenView
    {
        readonly Guid _ViewGUID = new Guid("cff8b896-1ad0-4c13-b51a-859f352385f4");
        public Guid ScreenGUID { get { return _ViewGUID; } }

        public MaskingSettingControl()
        {
            InitializeComponent();
        }
    }
}

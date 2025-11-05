using ProberInterfaces;
using System;
using System.Windows.Controls;

namespace MachineSettingStageView
{
    /// <summary>
    /// UserControl1.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MachineSettingStageView : UserControl, IMainScreenView
    {
        public MachineSettingStageView()
        {
            InitializeComponent();
        }

        private readonly Guid _ViewGUID = new Guid("e88db401-f69d-49ee-b4f4-dd7a1816d874");
        public Guid ScreenGUID
        {
            get { return _ViewGUID; }
        }
    }
}

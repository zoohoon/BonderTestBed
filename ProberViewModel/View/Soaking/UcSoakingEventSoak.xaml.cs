using ProberInterfaces;
using System;
using System.Windows.Controls;

namespace SoakingSettingView
{
    /// <summary>
    /// UcSoakingStep.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class UcSoakingEventSoak : UserControl, IMainScreenView
    {
        public UcSoakingEventSoak()
        {
            InitializeComponent();
        }

        readonly Guid _ViewGUID = new Guid("46181716-C8BE-4D26-A009-687C800A3DA3");
        public Guid ScreenGUID { get { return _ViewGUID; } }
    }
}

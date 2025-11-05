using ProberInterfaces;
using System;
using System.Windows.Controls;

namespace ForcedDoneView
{
    /// <summary>
    /// UserControl1.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class LotRunForcedDoneView : UserControl,IMainScreenView,IFactoryModule
    {
        public LotRunForcedDoneView()
        {
            InitializeComponent();
        }
        readonly Guid _ViewGUID = new Guid("589ECFAD-B887-4E38-A9E3-68FECA7E7513");
        public Guid ScreenGUID { get { return _ViewGUID; } }
    }


}

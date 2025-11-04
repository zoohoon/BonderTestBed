using ProberInterfaces;
using System;
using System.Windows.Controls;

namespace GPCardChangeOPView
{
    /// <summary>
    /// GPCardChageOPView.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class GPCardChageOPView : UserControl, IMainScreenView, IFactoryModule
    {
        public GPCardChageOPView()
        {
            InitializeComponent();
        }
        readonly Guid _ViewGUID = new Guid("6b0ae503-6f49-4d7e-95e3-e960a923f787");
        public Guid ScreenGUID { get { return _ViewGUID; } }
    }
}

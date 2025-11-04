using ProberInterfaces;
using System;
using System.Windows.Controls;

namespace ManualJogView
{
    /// <summary>
    /// ManualJogPage.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ManualJogPage : UserControl, IMainScreenView
    {
        public ManualJogPage()
        {
            InitializeComponent();
        }

        readonly Guid _ViewGUID = new Guid("51562D6C-D283-85E5-D743-350E2F0C8ABD");
        public Guid ScreenGUID { get { return _ViewGUID; } }
    }
}

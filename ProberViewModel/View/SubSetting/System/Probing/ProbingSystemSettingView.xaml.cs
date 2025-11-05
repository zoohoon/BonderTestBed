using ProberInterfaces;
using System;
using System.Windows.Controls;

namespace ProbingSystemSettingView
{
    /// <summary>
    /// UserControl1.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ProbingSystemSettingView : UserControl, IMainScreenView
    {
        public ProbingSystemSettingView()
        {
            InitializeComponent();
        }

        private readonly Guid _ViewGUID = new Guid("6d270b53-0fe3-40b2-831f-1bd8766ede86");
        public Guid ScreenGUID
        {
            get { return _ViewGUID; }
        }
    }
}

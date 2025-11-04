using ProberInterfaces;
using System;
using System.Windows.Controls;

namespace SoakingSystemView
{
    /// <summary>
    /// UserControl1.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class SoakingSystemView : UserControl, IMainScreenView
    {
        public SoakingSystemView()
        {
            InitializeComponent();
        }

        private readonly Guid _ViewGUID = new Guid("BF19307E-AB06-4F09-AC1E-27B348A8F216");
        public Guid ScreenGUID
        {
            get { return _ViewGUID; }
        }
    }
}

using System.Windows.Controls;

namespace NeedleCleanViewer
{
    using ProberInterfaces;
    /// <summary>
    /// NeedleCleanView.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class NeedleCleanView : UserControl, INeedleCleanView
    {
        public NeedleCleanView()
        {
            InitializeComponent();
        }
    }
}

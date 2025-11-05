using System.Windows.Controls;

namespace ProberDevelopPackWindow.Tab
{
    /// <summary>
    /// GemParamTransferView.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class GemParamTransferView : UserControl
    {
        GemParamTransferViewModel viewmodel;
        public GemParamTransferView()
        {
            viewmodel = new GemParamTransferViewModel();
            this.DataContext = viewmodel;
            InitializeComponent();
        }

        ~GemParamTransferView()
        {
            viewmodel = null;
        }
    }
}

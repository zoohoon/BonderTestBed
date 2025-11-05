using System.Windows.Controls;

namespace ProberDevelopPackWindow.Tab
{
    /// <summary>
    /// GemVidsEditorView.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class GemVidsEditorView : UserControl
    {
        GemVidsEditorViewModel viewmodel;
        public GemVidsEditorView()
        {
            viewmodel = new GemVidsEditorViewModel();
            this.DataContext = viewmodel;
            InitializeComponent();
        }

        ~GemVidsEditorView()
        {
            viewmodel = null;
        }
    }
}

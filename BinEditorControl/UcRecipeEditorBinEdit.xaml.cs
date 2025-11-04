using System.Windows;
using System.Windows.Controls;

namespace BinEditorControl
{
    /// <summary>
    /// UcRecipeEditorBinEdit.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class UcRecipeEditorBinEdit : UserControl
    {
        public UcRecipeEditorBinEdit()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            searchTextBox.Focus();
        }
    }
}

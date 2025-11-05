using System.Windows;
using System.Windows.Controls;

namespace RecipeEditorControl.RecipeEditorParamEdit
{
    /// <summary>
    /// UcRecipeEditorParamEdit.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class UcRecipeEditorParamEdit : UserControl
    {
        public UcRecipeEditorParamEdit()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            searchTextBox.Focus();
        }
        
    }
}

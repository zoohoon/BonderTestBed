using LogModule;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace RecipeEditorControl.RecipeEditorUC
{
    /// <summary>
    /// UcRecipeEditorCtxMenuButton.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class UcRecipeEditorCtxMenuButton : UserControl
    {
        public UcRecipeEditorCtxMenuButton()
        {
            InitializeComponent();

            this.DataContextChanged += UC_DataContextChanged;
        }

        private void UC_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            try
            {
                // TODO : 더 좋은 방법 없나?
                if (this.editBtn.ContextMenu != null)
                {
                    if (this.editBtn.ContextMenu.DataContext == null && this.DataContext != null)
                    {
                        this.editBtn.ContextMenu.DataContext = this.DataContext;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void editBtn_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                (sender as Border).ContextMenu.IsEnabled = true;
                (sender as Border).ContextMenu.PlacementTarget = (sender as Border);
                (sender as Border).ContextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
                (sender as Border).ContextMenu.IsOpen = true;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
    }
}

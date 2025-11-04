using LogModule;
using System;
using System.Windows.Controls;
using System.Windows.Input;

namespace BinEditorControl
{
    /// <summary>
    /// UcBinItem.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class UcBinItem : UserControl
    {
        public UcBinItem()
        {
            InitializeComponent();
        }

        private void contentBox_MouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                contentBox.Opacity = 0.5;
            }
            catch(Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        private void contentBox_MouseUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                contentBox.Opacity = 1;
            }
            catch(Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        private void contentBox_MouseLeave(object sender, MouseEventArgs e)
        {
            try
            {
                contentBox.Opacity = 1;
            }
            catch(Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
    }
}

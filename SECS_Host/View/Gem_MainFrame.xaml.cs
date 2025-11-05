using System;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Input;

namespace SECS_Host.View
{
    public partial class Gem_MainFrame : UserControl
    {
        View_Model.VM_MainFrameBase mainFrameBase;

        public Gem_MainFrame()
        {
            InitializeComponent();

            mainFrameBase = new View_Model.VM_MainFrameBase();
            this.DataContext = mainFrameBase;
        }

        private void lboxLog_SelectionChange(object sender, SelectionChangedEventArgs e)
        {
            if (lboxLog.SelectedItem != null)
                lboxLog.ScrollIntoView(lboxLog.SelectedItem);
        }

        internal void Closing(object sender, CancelEventArgs e)
        {
            mainFrameBase.Closing(sender, e);
        }

        private void DynamicReportTxtBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = true;
        }

        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (16 <= ((TextBox)sender).Text.Length)
                e.Handled = true;

            foreach (char c in e.Text)
            {
                if (!char.IsDigit(c))
                {
                    e.Handled = true;
                    break;
                }
            }
        }
    }
}
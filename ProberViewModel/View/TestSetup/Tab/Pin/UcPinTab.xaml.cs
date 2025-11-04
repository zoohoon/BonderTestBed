using System;
using System.Windows.Controls;
using System.Windows.Input;

namespace TestSetupDialog.Tab.Pin
{
    using LogModule;
    using System.Text.RegularExpressions;
    /// <summary>
    /// UcPinTab.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class UcPinTab : UserControl
    {
        public UcPinTab()
        {
            InitializeComponent();
        }

        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            try
            {

                TextBox tb = sender as TextBox;
                if (tb == null)
                    return;



                e.Handled = !IsTextAllowed(e.Text);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        private static readonly Regex _regex = new Regex("[^0-9.-]+"); //regex that matches disallowed text
        private static bool IsTextAllowed(string text)
        {
            return !_regex.IsMatch(text);
        }
    }
}

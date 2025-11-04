using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SoakingSettingView
{
    /// <summary>
    /// UcSoakingTemplateSoakingList.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class UcSoakingTemplateSoakingList : UserControl 
    {
        public object ItemsSource
        {
            get => GetValue(ItemsSourceProperty);
            set => SetValue(ItemsSourceProperty, value);
        }

        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register(
                "ItemsSource",
                typeof(object),
                typeof(UcSoakingTemplateSoakingList));


        public object SelectedItem
        {
            get => GetValue(SelectedItemProperty);
            set => SetValue(SelectedItemProperty, value);
        }

        public static readonly DependencyProperty SelectedItemProperty =
           DependencyProperty.Register(
               "SelectedItem",
               typeof(object),
               typeof(UcSoakingTemplateSoakingList));

        public UcSoakingTemplateSoakingList()
        {
            InitializeComponent();

        }
        private static readonly Regex _regex = new Regex("[^0-9.-]+"); //regex that matches disallowed text
        private static bool IsTextAllowed(string text)
        {
            return _regex.IsMatch(text);
        }
        private void TextBlock_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = IsTextAllowed(e.Text);
        }
    }

}

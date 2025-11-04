using System.Windows;
using System.Windows.Controls;

namespace SoakingSettingView
{
    /// <summary>
    /// UcSoakingTemplateChillingList.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class UcSoakingTemplateChillingList : UserControl
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
                typeof(UcSoakingTemplateChillingList));


        public object SelectedItem
        {
            get => GetValue(SelectedItemProperty);
            set => SetValue(SelectedItemProperty, value);
        }

        public static readonly DependencyProperty SelectedItemProperty =
           DependencyProperty.Register(
               "SelectedItem",
               typeof(object),
               typeof(UcSoakingTemplateChillingList));
        public UcSoakingTemplateChillingList()
        {
            InitializeComponent();
        }

    }

  
}

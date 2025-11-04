using MahApps.Metro.Controls.Dialogs;
using System;

namespace SoakingSettingView
{
    /// <summary>
    /// UcSoakingAdvancedSetting.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class UcSoakingAdvancedSetting : CustomDialog
    {
        public UcSoakingAdvancedSetting()
        {
            InitializeComponent();
        }

        private void Apply_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            ApplyAlert.Visibility = System.Windows.Visibility.Visible;
            ApplyButton.Focus();
        }

        private void ApplyClose_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            ApplyAlert.Visibility = System.Windows.Visibility.Hidden;
        }
    }


    public class EnumFilter
    {
        public Array LightTypeFilterValues(Type enumType)
        {
            Array allValues = Enum.GetValues(enumType);
            object[] filteredValues = new object[allValues.Length - 1];
            for (int i = 0; i < allValues.Length - 1; i++)
            {
                filteredValues[i] = allValues.GetValue(i);
            }

            return filteredValues;
        }
    }
}

using System;
using System.Windows.Data;

namespace HighMagAdvanceSetup.View
{
    using HighMagAdvanceSetup.ViewModel;
    using MahApps.Metro.Controls.Dialogs;
    using System.ComponentModel;
    using System.Globalization;
    using System.Reflection;
    using System.Windows.Input;

    /// <summary>
    /// HighMagStandardAdvanceSetupView.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class HighMagStandardAdvanceSetupView : CustomDialog
    {
        public HighMagStandardAdvanceSetupView()
        {
            InitializeComponent();
            this.KeyDown += HighMagStandardAdvanceSetupView_KeyDown;
        }

        private void HighMagStandardAdvanceSetupView_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.KeyboardDevice.IsKeyDown(Key.LeftCtrl) || e.KeyboardDevice.IsKeyDown(Key.RightCtrl))
            {
                if (e.KeyboardDevice.IsKeyDown(Key.H))
                {
                    var viewModel = this.DataContext as HighMagStandardAdvanceSetupViewModel;

                    if (viewModel != null)
                    {
                        viewModel.HiddenTabVisibility = System.Windows.Visibility.Visible;
                    }
                }
            }
        }
    }

    public class EnabledDisabledToBooleanConverter : IValueConverter
    {
        private const string EnabledText = "Enabled";
        private const string DisabledText = "Disabled";
        public static readonly EnabledDisabledToBooleanConverter Instance = new EnabledDisabledToBooleanConverter();

        private EnabledDisabledToBooleanConverter()
        {
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Equals(true, value)
                ? EnabledText
                : DisabledText;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            //Actually won't be used, but in case you need that
            return Equals(value, EnabledText);
        }
    }

    public class EnumDescriptionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return null;

            FieldInfo fieldInfo = value.GetType().GetField(value.ToString());
            DescriptionAttribute[] attributes = (DescriptionAttribute[])fieldInfo.GetCustomAttributes(typeof(DescriptionAttribute), false);

            return (attributes.Length > 0) ? attributes[0].Description : value.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            foreach (FieldInfo fieldInfo in targetType.GetFields())
            {
                DescriptionAttribute[] attributes = (DescriptionAttribute[])fieldInfo.GetCustomAttributes(typeof(DescriptionAttribute), false);
                if (attributes.Length > 0 && attributes[0].Description == value.ToString())
                {
                    return Enum.Parse(targetType, fieldInfo.Name);
                }
            }
            return Enum.Parse(targetType, value.ToString());
        }
    }
}

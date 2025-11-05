namespace LowMagAdvanceSetup.View
{
    using MahApps.Metro.Controls.Dialogs;
    using System;
    using System.ComponentModel;
    using System.Globalization;
    using System.Reflection;
    using System.Windows.Data;

    /// <summary>
    /// LowMagStandardAdvanceSetupView.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class LowMagStandardAdvanceSetupView : CustomDialog
    {
        public LowMagStandardAdvanceSetupView()
        {
            InitializeComponent();
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

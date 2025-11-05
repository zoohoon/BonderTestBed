using MahApps.Metro.Controls.Dialogs;
using System;
using System.Globalization;
using System.Windows.Data;

namespace BaseFocusingAdvanceSetup.View
{
    /// <summary>
    /// BaseFocusingAdvanceSetupView.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class BaseFocusingAdvanceSetupView : CustomDialog
    {
        public BaseFocusingAdvanceSetupView()
        {
            InitializeComponent();
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
}

using ProberInterfaces;
using System;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;

namespace ProberViewModel.View
{
    public enum AdvancedOption
    {
        NotUse,
        RegPin,
        RegKey,
        RegPinAndKey
    }

    /// <summary>
    /// DigitalTwinPage.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class DigitalTwinView : UserControl, IMainScreenView, IFactoryModule
    {
        public DigitalTwinView()
        {
            InitializeComponent();
        }

        readonly Guid _ViewGUID = new Guid("f4a0569c-b2a3-4190-9b4c-b4a44f060773");
        public Guid ScreenGUID { get { return _ViewGUID; } }
    }

    public class EnumToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (parameter is null || value is null)
            {
                return false;
            }
            string parameterString = parameter.ToString();
            if (parameterString == value.ToString())
            {
                return true;
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (parameter is null)
            {
                return Binding.DoNothing;
            }
            return Enum.Parse(targetType, parameter.ToString());
        }
    }

    public class FocusingRangeValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (int.TryParse((string)value, out int number))
            {
                if (number >= 0 && number <= 1000)
                {
                    return ValidationResult.ValidResult;
                }
            }

            return new ValidationResult(false, "Value must be between 0 and 1000.");
        }
    }

    public class LightRangeValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (int.TryParse((string)value, out int number))
            {
                if (number >= 0 && number <= 255)
                {
                    return ValidationResult.ValidResult;
                }
            }

            return new ValidationResult(false, "Value must be between 0 and 255.");
        }
    }
}

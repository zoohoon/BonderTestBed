using ProberInterfaces;
using System;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace UcContactSettingView
{
    public partial class ContactSettingView : UserControl, IMainScreenView
    {
        private Guid _ViewGUID = new Guid("D13FC548-BCE1-4C4E-B1E3-413A445DB9F0");
        public Guid ScreenGUID { get { return _ViewGUID; } }

        public ContactSettingView()
        {
            InitializeComponent();
        }
    }

    public class BoolToColorConverter : IValueConverter
    {
        public object Convert(
         object value, Type targetType,
         object parameter, System.Globalization.CultureInfo culture)
        {
            Brush retBrush = Brushes.Gray;
            try
            {

                if (value is bool)
                {
                    bool bValue = (bool)value;

                    if (bValue == true)
                    {
                        retBrush = Brushes.LightGreen;
                    }
                }

            }
            catch (Exception err)
            {
                throw;
            }
            return retBrush;
        }

        public object ConvertBack(
         object value, Type targetType,
         object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                // I don't think you'll need this
                throw new Exception("Can't convert back");
            }
            catch (Exception err)
            {
                throw;
            }
        }
    }

    public class FirstContactAllContactSubCal : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            double retVal = 0.0;

            try
            {
                if (2 <= values.Length)
                {
                    if (values[0] is double && values[1] is double)
                    {
                        double firContactHeight = (double)values[0];
                        double allContactHeight = (double)values[1];

                        retVal = allContactHeight - firContactHeight;
                    }
                }
            }
            catch (Exception err)
            {
                throw;
            }

            return retVal;
        }

        public object[] ConvertBack(object value, Type[] targetType, object parameter, CultureInfo culture)
        {
            throw new Exception("Can't convert back");
        }
    }

    public class EnumToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null)
            {
                return false; // or return parameter.Equals(YourEnumType.SomeDefaultValue);
            }
            return value.ToString().Equals(parameter.ToString());
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value.Equals(true) ? parameter : Binding.DoNothing;
        }
    }

    public class ListViewConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            return values.Clone();
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}

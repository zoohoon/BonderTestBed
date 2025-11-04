using ProberInterfaces;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ProberViewModel
{
    /// <summary>
    /// AccuracyCheckSetup.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class AccuracyCheckSetup : UserControl, IMainScreenView
    {
        public AccuracyCheckSetup()
        {
            InitializeComponent();
        }

        private readonly Guid _ViewGUID = new Guid("7ecc6dde-63e4-4bd8-a4a9-4e982afb831a");
        public Guid ScreenGUID
        {
            get { return _ViewGUID; }
        }
    }

    public class BoolToStringConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values[0] is bool bVal)
                return bVal ? "O" : "X";
            return string.Empty;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class BoolToColorConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values[0] is bool bVal)
                return new SolidColorBrush(bVal ? Colors.LimeGreen : Colors.Red);
            return new SolidColorBrush(Colors.Black);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class OffsetToColorConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length != 2
            || !(values[0] is string diffString)
            || !double.TryParse(diffString, out double difference)
            || !(values[1] is double spec))
            {
                return Brushes.Black;  // Default color if binding fails
            }

            return Math.Abs(difference) <= spec ? Brushes.LimeGreen : Brushes.Red;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class IncrementConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int index)
            {
                return index + 1;
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}

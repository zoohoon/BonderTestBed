using LogModule;
using System;
using System.Globalization;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Data;

namespace PolishWaferSourceSettingViewProject.UC
{
    /// <summary>
    /// PolisWaferSourceSelectionView.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class PolisWaferSourceSelectionView : UserControl
    {
        public PolisWaferSourceSelectionView()
        {
            InitializeComponent();
        }
    }

    public class MultiplyFormulaStringConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var intValues = values.Cast<int>().ToArray();

            //var leftPart = string.Join(" x ", intValues);

            //var rightPart = intValues.Sum().ToString();

            //var result = string.Format("{0} = {1}", leftPart, rightPart);
            var result = $"{intValues[0] + 1} / {intValues[1]}";

            return result;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class CountNumConverter : IValueConverter
    {
        object IValueConverter.Convert(object value,
            Type targetType,
            object parameter,
            System.Globalization.CultureInfo culture)
        {
            string countStr = string.Empty;

            try
            {
                if (value != null && value is int)
                {
                    int count = (int)value;
                    if (count == 0)
                    {
                        countStr = "1st";
                    }
                    else if (count == 1)
                    {
                        countStr = "2nd";
                    }
                    else if (count == 2)
                    {
                        countStr = "3rd";
                    }
                    else
                    {
                        countStr = (count + 1).ToString() + "th";
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Debug("[ValueConverter - CountNumConverter] Can't convert");
                LoggerManager.Exception(err);
            }

            return countStr;
        }

        object IValueConverter.ConvertBack(object value,
            Type targetType,
            object parameter,
            System.Globalization.CultureInfo culture)
        {
            throw new Exception("The method or operation is not implemented.");
        }
    }
}

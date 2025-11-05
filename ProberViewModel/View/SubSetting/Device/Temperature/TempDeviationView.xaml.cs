using LogModule;
using ProberInterfaces;
using System;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;
using ProberInterfaces.Temperature;

namespace UcTempDeviationView
{
    public partial class TempDeviationView : UserControl, IMainScreenView
    {
        public Guid ScreenGUID { get; } = new Guid("17F02446-D73D-4E02-9685-0EA1F4569E5F");
        public TempDeviationView()
        {
            InitializeComponent();
        }
    }

    public class DeviationPresentationConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double retVal = 0;
            bool parseResult = false;

            try
            {
                parseResult = double.TryParse(value.ToString(), out retVal);

                //if(parseResult == true)
                //{
                //    retVal;
                //}
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

            return retVal;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double retVal = 0;
            bool parseResult = false;

            try
            {
                parseResult = double.TryParse(value.ToString(), out retVal);

                if (parseResult == true)
                {
                    retVal *= 10;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

            return retVal;
        }
    }
}

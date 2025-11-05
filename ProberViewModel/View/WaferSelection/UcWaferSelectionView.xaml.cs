using ProberInterfaces;
using System;
using System.Windows.Controls;
using System.Windows.Data;
using LogModule;

namespace ProberViewModel
{
    public partial class UcWaferSelectionView : UserControl, IMainScreenView
    {
        public UcWaferSelectionView()
        {
            InitializeComponent();
        }

        readonly Guid _ViewGUID = new Guid("02cf70d9-f2d3-4180-9efc-6fc8e5a22041");
        public Guid ScreenGUID { get { return _ViewGUID; } }
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
                    if(count == 0)
                    {
                        countStr = "1st";
                    }
                    else if(count == 1)
                    {
                        countStr = "2nd";
                    }
                    else if(count == 2)
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

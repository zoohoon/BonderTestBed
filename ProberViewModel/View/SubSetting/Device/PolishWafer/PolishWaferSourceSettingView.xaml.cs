using LogModule;
using ProberInterfaces;
using System;
using System.Windows.Controls;
using System.Windows.Data;

namespace UcPolishWaferSourceSettingView
{
    /// <summary>
    /// UserControl1.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class PolishWaferSourceSettingView : UserControl, IMainScreenView
    {
        private readonly Guid _ViewGUID = new Guid("5b15248a-8999-481b-b2d9-41e4dad41959");
        public Guid ScreenGUID
        {
            get { return _ViewGUID; }
        }

        public PolishWaferSourceSettingView()
        {
            InitializeComponent();
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

    public class InspectionTrayCountNumConverter : IValueConverter
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

    public class FixedTrayCountNumConverter : IValueConverter
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
                        countStr = "Fixed Tray 1st";
                    }
                    else if (count == 1)
                    {
                        countStr = "Fixed Tray 2nd";
                    }
                    else if (count == 2)
                    {
                        countStr = "Fixed Tray 3rd";
                    }
                    else
                    {
                        countStr = $"Fixed Tray {(count + 1).ToString()}th";
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

namespace ValueConverters
{
    using LogModule;
    using ProberInterfaces;
    using System;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Data;
    using System.Windows.Media.Imaging;

    public class ImageSourceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            BitmapImage retVal = new BitmapImage(new Uri("pack://application:,,,/ImageResourcePack;component/Images/UnLock.png"));
            try
            {
                if ((StageLockMode)value == StageLockMode.LOCK)
                {
                    retVal = new BitmapImage(new Uri("pack://application:,,,/ImageResourcePack;component/Images/Lock.png"));
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class LockModeToColorConverter : IValueConverter
    {
        static string BLUE = "#99B7FF";
        static string YELLOW = "#FFAB7C";
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string retval = BLUE;
            try
            {
                if ((StageLockMode)value == StageLockMode.LOCK)
                {
                    retval = YELLOW;
                }
                else
                {
                    retval = BLUE;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retval;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class LockModeToVisibleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Visibility retval = Visibility.Collapsed;
            try
            {
                if ((StageLockMode)value == StageLockMode.LOCK)
                {
                    retval = Visibility.Visible;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retval;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

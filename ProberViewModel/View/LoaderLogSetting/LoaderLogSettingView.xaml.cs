using LoaderLogSettingViewModelModule;
using LogModule;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace LoaderLogSettingViewModule
{
    /// <summary>
    /// LoaderLogSettingView.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class LoaderLogSettingView : Window
    {
        LoaderLogSettingViewModel ViewModel;
        public LoaderLogSettingView()
        {
            InitializeComponent();
            ViewModel = new LoaderLogSettingViewModel();
            this.DataContext = ViewModel;
        }
    }
    public class ImageFormatConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return false;
            }
            return value.Equals(parameter);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value.Equals(true) ? parameter : Binding.DoNothing;
        }
    }

    public class GetIndexConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string retVal = "";
            try
            {
                if (value != null || value is int)
                {
                    retVal = $"C{value}";
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
            return null;
        }
    }
}

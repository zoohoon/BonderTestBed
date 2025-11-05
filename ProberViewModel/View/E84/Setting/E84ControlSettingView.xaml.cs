namespace ProberViewModel.View.E84.Setting
{
    using ProberInterfaces;
    using System;
    using System.Globalization;
    using System.Windows.Controls;
    using System.Windows.Data;
    using System.Windows.Media;
    /// <summary>
    /// E84ControlSettingView.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class E84ControlSettingView : UserControl
    {
        public E84ControlSettingView()
        {
            InitializeComponent();
        }
    }

    public class MultiBindingParamConverter : IMultiValueConverter
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

    public class BoolToIntConverter : IValueConverter
    {
        public object Convert(object value, Type targetType,
                              object parameter, CultureInfo culture)
        {
            if (value is bool)
            {
                bool flag = (bool)value;
                if (flag)
                    return 1;
                else
                    return 0;
            }
            return "---";
        }
        public object ConvertBack(object value, Type targetType,
                                  object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    public class ModeChangeEnableConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values[0] is ModuleStateEnum && values[1] is bool)
            {
                ModuleStateEnum moduleState = (ModuleStateEnum)values[0];
                bool isDisconnect = (bool)values[1];
                if (moduleState == ModuleStateEnum.RUNNING || isDisconnect)
                    return false;
                else
                    return true;
            }
            return true;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
    public class BoolToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType,
                              object parameter, CultureInfo culture)
        {
            if (value is bool)
            {
                bool var = (bool)value;
                if (var == true)
                    return Brushes.Green;
                else
                    return Brushes.Red;
            }
            return true;
        }
        public object ConvertBack(object value, Type targetType,
                                  object parameter, CultureInfo culture)
        {
            return null;
        }
    }
    public class BoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType,
                              object parameter, CultureInfo culture)
        {
            if (value is bool)
            {
                bool var = (bool)value;
                return !var;

            }
            return true;
        }
        public object ConvertBack(object value, Type targetType,
                                  object parameter, CultureInfo culture)
        {
            return null;
        }
    }
    public class ConnectIsEnableConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values[0] is bool && values[1] is bool)
            {
                bool var1 = (bool)values[0];
                bool var2 = (bool)values[1];
                if (!var1)
                {
                    return false;
                }
                else
                {
                    return var2;
                }
            }
            return true;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}

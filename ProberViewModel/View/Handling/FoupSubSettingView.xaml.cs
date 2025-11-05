namespace ProberViewModel.View.Handling
{
    using ProberInterfaces.Foup;
    using ProberInterfaces;
    using System;
    using System.Globalization;
    using System.Windows.Controls;
    using System.Windows.Data;
    using System.Windows;

    /// <summary>
    /// FoupSubSettingView.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class FoupSubSettingView : UserControl
    {
        public FoupSubSettingView()
        {
            InitializeComponent();
        }
    }

    public class ModeToBtnEnableConverter : IValueConverter
    {
        public object Convert(object value, Type targetType,
                              object parameter, CultureInfo culture)
        {
            if (value is FoupModeStatusEnum)
            {
                FoupModeStatusEnum targetEnum = (FoupModeStatusEnum)value;
                if (parameter is FoupModeStatusEnum)
                {
                    FoupModeStatusEnum destEnum = (FoupModeStatusEnum)parameter;
                    if(targetEnum == FoupModeStatusEnum.ONLINE)
                    {
                        if (destEnum == FoupModeStatusEnum.ONLINE)
                            return false;
                        else if (destEnum == FoupModeStatusEnum.OFFLINE)
                            return true;
                    }
                    else if(targetEnum == FoupModeStatusEnum.OFFLINE)
                    {
                        if (destEnum == FoupModeStatusEnum.ONLINE)
                            return true;
                        else if (destEnum == FoupModeStatusEnum.OFFLINE)
                            return false;
                    }
                }
            }
            return true;
        }
        public object ConvertBack(object value, Type targetType,
                                  object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    public class StateToGBEnableConverter : IValueConverter
    {
        public object Convert(object value, Type targetType,
                              object parameter, CultureInfo culture)
        {

            if (value is DynamicModeEnum)
            {
                DynamicModeEnum dynamicFoupState = (DynamicModeEnum)value;
                if (dynamicFoupState == DynamicModeEnum.NORMAL)
                    return false;
                else if (dynamicFoupState == DynamicModeEnum.DYNAMIC)
                    return true;
                
            }
            return false;
            
        }
        public object ConvertBack(object value, Type targetType,
                                  object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    public class FoupStateToInverseVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return Visibility.Collapsed;

            FoupStateEnum foupState = (FoupStateEnum)value;

            if (foupState == FoupStateEnum.ERROR || foupState == FoupStateEnum.UNLOADING || foupState == FoupStateEnum.LOADING)
                return Visibility.Visible;
            else
                return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}

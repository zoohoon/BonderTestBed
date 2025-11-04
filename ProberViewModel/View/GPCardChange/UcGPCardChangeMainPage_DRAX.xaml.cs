using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace GPCardChangeMainPageView
{
    using LogModule;
    using ProberInterfaces;
    using ProberInterfaces.CardChange;
    using System.Globalization;
    using System.Windows.Media;

    /// <summary>
    /// UcGPCardChangeMainPage.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class UcGPCardChangeMainPage_DRAX : UserControl, IMainScreenView, IFactoryModule
    {
        public UcGPCardChangeMainPage_DRAX()
        {
            InitializeComponent();
        }
        readonly Guid _ViewGUID = new Guid("b094fbf9-35a0-43ab-9311-def5a717a9f7");
        public Guid ScreenGUID { get { return _ViewGUID; } }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {

            //==> Display port 화면을 출력 시키기 위해.
            this.VisionManager()?.SetDisplayChannelStageCameras(displayport);

        }
    }

    public class PogoAlignPointConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                EnumPogoAlignPoint param = (EnumPogoAlignPoint)parameter;
                if(param == EnumPogoAlignPoint.POINT_4)
                {
                    if((EnumPogoAlignPoint)value == param)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else if(param == EnumPogoAlignPoint.POINT_3)
                {
                    if ((EnumPogoAlignPoint)value == param)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    LoggerManager.Debug($"CardChangeSysParam.PogoAlignPoint.Value is INVALID");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value.Equals(true) ? parameter : Binding.DoNothing;
        }
    }

    public class EllipseInputFill : IValueConverter
    {
        SolidColorBrush DeepSkyBlueColor = new SolidColorBrush(Colors.DeepSkyBlue);
        SolidColorBrush DimGrayColor = new SolidColorBrush(Colors.DimGray);
        SolidColorBrush RedColor = new SolidColorBrush(Colors.Red);
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool)
            {
                if ((bool)value)
                {
                    return DeepSkyBlueColor;
                }
                else
                {
                    return DimGrayColor;
                }
            }
            else
            {
                return RedColor;
            }
            return RedColor;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class InputStringConvert : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool)
            {
                if ((bool)value)
                {
                    return "ON";
                }
                else
                {
                    return "OFF";
                }
            }
            else
            {
                return "ERROR";
            }
            return "ERROR";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class EllipseInputStroke : IValueConverter
    {
        SolidColorBrush SkyBlueColor = new SolidColorBrush(Colors.SkyBlue);
        SolidColorBrush LightGrayColor = new SolidColorBrush(Colors.LightGray);
        SolidColorBrush RedColor = new SolidColorBrush(Colors.Red);
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool)
            {
                if ((bool)value)
                {
                    return SkyBlueColor;
                }
                else
                {
                    return LightGrayColor;
                }
            }
            else
            {
                return RedColor;
            }
            return RedColor;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

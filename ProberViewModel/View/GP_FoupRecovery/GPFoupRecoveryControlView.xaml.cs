using LogModule;
using ProberInterfaces;
using ProberInterfaces.Foup;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    /// GPFocupRecoveryControlView.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class GPFocupRecoveryControlView : UserControl, IMainScreenView
    {
        readonly Guid _ViewGUID = new Guid("F7BE0142-1ED7-4483-A257-AC512454F6F2");
        public Guid ScreenGUID { get { return _ViewGUID; } }

        public GPFocupRecoveryControlView()
        {
            InitializeComponent();
        }

        private static GPFocupRecoveryControlView foupRecoveryControlView;

        public static GPFocupRecoveryControlView GetInstance()
        {
            if (foupRecoveryControlView == null)
            {
                foupRecoveryControlView = new GPFocupRecoveryControlView();
            }
            return foupRecoveryControlView;
        }
        //private void Grid_MouseMove(object sender, MouseEventArgs e)
        //{
        //    InputGrid.Visibility = Visibility.Visible;
        //}

        //private void Grid_MouseLeave(object sender, MouseEventArgs e)
        //{
        //    InputGrid.Visibility = Visibility.Hidden;
        //}
    }

    public class SelectedItemToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // 선택된 값이 null이 아니면 true를 반환하고, 그렇지 않으면 false를 반환합니다.
            return value != null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class RectGreenFill : IValueConverter
    {
        SolidColorBrush GreenColor = new SolidColorBrush(Colors.LawnGreen);
        SolidColorBrush BlackColor = new SolidColorBrush(Colors.Black);
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool)
            {
                if ((bool)value)
                {
                    return GreenColor;
                }
                else
                {
                    return BlackColor;
                }
            }
            else
            {
                return BlackColor;
            }
            return BlackColor;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class RectOrangeFill : IValueConverter
    {
        SolidColorBrush OrangeColor = new SolidColorBrush(Colors.Orange);
        SolidColorBrush BlackColor = new SolidColorBrush(Colors.Black);
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool)
            {
                if ((bool)value)
                {
                    return OrangeColor;
                }
                else
                {
                    return BlackColor;
                }
            }
            else
            {
                return BlackColor;
            }
            return BlackColor;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class RectRedFill : IValueConverter
    {
        SolidColorBrush RedColor = new SolidColorBrush(Colors.Red);
        SolidColorBrush BlackColor = new SolidColorBrush(Colors.Black);
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool)
            {
                if ((bool)value)
                {
                    return RedColor;
                }
                else
                {
                    return BlackColor;
                }
            }
            else
            {
                return BlackColor;
            }
            return BlackColor;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class EllipseInputFill : IValueConverter
    {
         SolidColorBrush DeepSkyBlueColor = new SolidColorBrush(Colors.DeepSkyBlue);
         SolidColorBrush DimGrayColor = new SolidColorBrush(Colors.DimGray);
         SolidColorBrush RedColor = new SolidColorBrush(Colors.Red);
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(value is bool)
            {
                if((bool)value)
                {
                    return DeepSkyBlueColor;
                }
                else
                {
                    return DimGrayColor;
                }
            }else
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
        public class InputStringColor : IValueConverter
    {
        SolidColorBrush BlackColor = new SolidColorBrush(Colors.Black);
        SolidColorBrush DimGrayColor = new SolidColorBrush(Colors.DimGray);
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool)
            {
                if ((bool)value)
                {
                    return BlackColor;
                }
                else
                {
                    return DimGrayColor;
                }
            }
            else
            {
                return DimGrayColor;
            }
            return DimGrayColor;
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
    public class IsActiveBrushConvert : IValueConverter
    {
        SolidColorBrush LightGray = new SolidColorBrush(Colors.LightGray);
        SolidColorBrush OrangeColor = new SolidColorBrush(Colors.Orange);
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool)
            {
                if ((bool)value)
                {
                    return OrangeColor;
                }
                else
                {
                    return LightGray;
                }
            }
            else
            {
                return LightGray;
            }
            return LightGray;
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

    public class SelectedIndexToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int selectedIndex && parameter is string targetIndexString)
            {
                if (int.TryParse(targetIndexString, out int targetIndex))
                {
                    return selectedIndex == targetIndex;
                }
            }

            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class BooleanToInverseConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return !boolValue;
            }

            return Binding.DoNothing;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    public class BooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return boolValue ? Visibility.Visible : Visibility.Hidden;
            }

            return Visibility.Hidden;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
    public class BooleanToMaxWidthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return boolValue ? 1000 : 0;
            }

            return 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    public class CassetteTypeEnumToStringConverter : IValueConverter
    {
        public object Convert(object value, Type TargetType, object parameter, CultureInfo culture)
        {             
            string retval = string.Empty;
            try
            {
                if (value != null)
                {
                    if(value is CassetteTypeEnum)
                    {
                        retval = value.ToString();
                    }               
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
            throw new NotSupportedException();
        }
    }

}

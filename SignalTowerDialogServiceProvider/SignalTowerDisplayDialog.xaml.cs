using ProberInterfaces.SignalTower;
using SignalTowerModule;
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

namespace SignalTowerDialogServiceProvider
{
    /// <summary>
    /// SignalTowerDisplayDialog.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class SignalTowerDisplayDialog : Window
    {
        private SignalTowerDisplayDialogService _ViewModel { get; set; }
        public SignalTowerDisplayDialog()
        {
            InitializeComponent();
            _ViewModel = new SignalTowerDisplayDialogService();              
            this.DataContext = _ViewModel;
        } 
    }

    public class ColorToBrushConverter : IValueConverter
    {
        public object Convert(object values, Type targetType, object parameter, CultureInfo culture)
        {            
            if (values != null)
            {
                if (values is Color)
                {
                    SolidColorBrush solidcolorBrush = new SolidColorBrush((Color)values);
                    return solidcolorBrush;
                }
            }            
            return null;
        }

        public object ConvertBack(object values, Type targetType, object parameter, CultureInfo culture)
        {
            throw new Exception("The method or operation is not implemented.");
        }
    }

    public class ListViewOpacityConverter : IMultiValueConverter
    {
        public object Convert(object[] value, Type targetType, object parameter, CultureInfo culture)
        {
            double opacity = 0.0;
            try
            {
                if (value != null)
                {
                    if(value[0] is int && value[1] is int)
                    {
                        var oncount = (int)value[0];
                        var blinkcount = (int)value[1];
                        //if (blinkcount > 0)
                        //{
                        //    opacity = 0;
                        //}
                        if (oncount > 0)
                        {
                            opacity = 0.9;
                        }
                        else if (oncount == 0 && blinkcount == 0)
                        {
                            opacity = 0.0;
                        }
                    }
                }
            }
            catch (Exception)
            {
            }
            return opacity;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    public class ListViewConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool enable = false;
            if(value != null)
            {
                if((int)value == 0)
                {
                    enable = false;
                }
                else
                {
                    enable = true;
                }
            }
            return enable;
        }

        public object ConvertBack(object value, Type targetTypes, object parameter, CultureInfo culture)
        {
            throw new Exception("The method or operation is not implemented.");
        }
    }

    public class StringToVisibilityConverter : System.Windows.Markup.MarkupExtension, IValueConverter
    { 
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value as string == "Lamp")
                return Visibility.Visible;
            else if (value as string == "Buzzer")
                return Visibility.Collapsed;
            else
                return string.IsNullOrEmpty(value as string);

                //return string.IsNullOrEmpty(value as string)
                //? Visibility.Collapsed : Visibility.Visible;
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }    
}

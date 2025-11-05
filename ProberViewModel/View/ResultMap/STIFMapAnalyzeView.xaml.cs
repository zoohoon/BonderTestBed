using ProberInterfaces;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace ProberViewModel
{
    /// <summary>
    /// BinAnalyzeView.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class STIFMapAnalyzeView : UserControl, IMainScreenView
    {
        readonly Guid _ViewGUID = new Guid("2c30a55f-2c44-4451-9f54-3623edf02231");
        public Guid ScreenGUID { get { return _ViewGUID; } }

        public STIFMapAnalyzeView()
        {
            InitializeComponent();
        }
    }

    public class ColorToSolidColorBrushValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (null == value)
            {
                return null;
            }

            // For a more sophisticated converter, check also the targetType and react accordingly..
            if (value is Color)
            {
                Color color = (Color)value;
                return new SolidColorBrush(color);
            }
            // You can support here more source types if you wish
            // For the example I throw an exception

            Type type = value.GetType();
            throw new InvalidOperationException("Unsupported type [" + type.Name + "]");

            //if(value is SolidColorBrush)
            //{
            //    SolidColorBrush inputvalue = value as SolidColorBrush;

            //    //System.Drawing.Color myColor = System.Drawing.Color.FromArgb(inputvalue.Color.A,
            //    //                                             inputvalue.Color.R,
            //    //                                             inputvalue.Color.G,
            //    //                                             inputvalue.Color.B);

            //    System.Windows.Media.Color

            //    return myColor;
            //}
            //else
            //{
            //    return null;
            //}
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            // If necessary, here you can convert back. Check if which brush it is (if its one),
            // get its Color-value and return it.

            throw new NotImplementedException();
        }
    }

    public class HandlingEventTrigger : System.Windows.Interactivity.EventTrigger
    {
        protected override void OnEvent(System.EventArgs eventArgs)
        {
            var routedEventArgs = eventArgs as RoutedEventArgs;
            if (routedEventArgs != null)
                routedEventArgs.Handled = true;

            base.OnEvent(eventArgs);
        }
    }
}

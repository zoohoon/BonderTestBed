using ProberErrorCode;
using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;

namespace AlarmViewDialog
{
    /// <summary>
    /// ImageViewerWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ImageViewerWindow : Window
    {
        public ImageViewerWindow()
        {
            InitializeComponent();
        }
    }
    public class IndexConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            ListBoxItem item = value as ListBoxItem;
            ListBox listBox = ItemsControl.ItemsControlFromItemContainer(item) as ListBox;
            int index = listBox.ItemContainerGenerator.IndexFromContainer(item);
            return index + 1; // 1-based index
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class EventCodeToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is EventCodeEnum eventCode)
            {
                return eventCode != EventCodeEnum.NONE ? Brushes.Red : Brushes.LimeGreen;
            }
            return Brushes.White;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

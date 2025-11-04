using ProberInterfaces;
using System;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;

namespace OCRView
{
    /// <summary>
    /// UcOCRSetting.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class UcOCRSetting : UserControl, IMainScreenView
    {
        public UcOCRSetting()
        {
            InitializeComponent();
        }
        private Autofac.IContainer Container { get; set; }

        readonly Guid _ViewGUID = new Guid("5142BD1C-E64B-51F5-29CE-50620BED445A");
        public Guid ScreenGUID { get { return _ViewGUID; } }
    }

    public class BooleanToVisiblityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool && parameter is bool)
            {
                bool isCurrStateValue = (bool)value;
                bool isVisibleValue = (bool)parameter;

                if (isCurrStateValue == isVisibleValue)
                    return System.Windows.Visibility.Visible;
                else
                    return System.Windows.Visibility.Hidden;
            }
            return System.Windows.Visibility.Hidden;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

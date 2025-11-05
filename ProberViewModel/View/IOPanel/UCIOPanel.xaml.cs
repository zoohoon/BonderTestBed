using ProberInterfaces;
using System;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;

namespace UCIOPanel
{
    /// <summary>
    /// UCIOPanel.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class UCIOPanel : UserControl, IMainScreenView
    {
        public UCIOPanel()
        {
            InitializeComponent();
        }

        readonly Guid _ViewGUID = new Guid("10CA2446-3785-44C1-AB07-E292036B82EA");
        public Guid ScreenGUID { get { return _ViewGUID; } }
    }

    public class MaterialsConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            string retVal = null;
            if (values[0] != null && values[1] != null)
            {
                var str1 = values[0].ToString();
                var str2 = values[1].ToString();
                retVal = "(ch: " + str1 + ", port: " + str2 + " );";
            }

            return retVal;
        }

        public object[] ConvertBack(
            object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}


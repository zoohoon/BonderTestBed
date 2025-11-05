namespace ValueConverters
{
    using System.Globalization;
    using System.Linq;
    using System.Windows.Data;
    using System;

    public class ConvertMultiValue : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            //if (values != null)
            //{
            //    WaferAlignInfomation info = new WaferAlignInfomation();

            //    if (values[1] != DependencyProperty.UnsetValue) info.DieSizeX = Double.Parse(values[1].ToString());
            //    if (values[2] != DependencyProperty.UnsetValue) info.DieSizeX = Double.Parse(values[2].ToString());

            //    return info;
            //}
            //else
            //{
            //    return null;
            //}
             return values.ToArray(); ;

        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
   
}

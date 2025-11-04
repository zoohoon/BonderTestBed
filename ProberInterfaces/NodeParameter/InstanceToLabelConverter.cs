using System;
using System.Globalization;
using System.Windows.Data;

namespace ProberInterfaces
{
    public class InstanceToLabelConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is IParamNode && value != null)
            {
                var obj = value as IParamNode;
                if (obj.Genealogy == null)
                {
                    return obj.GetType().Name;
                }
                else
                {
                    return obj.Genealogy;
                }
            }
            else
            {
                if (value == null)
                {
                    return "null";
                }
                else
                {
                    return value.GetType().Name;
                }
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

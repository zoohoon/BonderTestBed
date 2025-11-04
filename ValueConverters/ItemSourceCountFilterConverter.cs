using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;
using LogModule;

namespace ValueConverters
{
    //[ValueConversion(typeof(bool), typeof(Visibility))]
    //[MarkupExtensionReturnType(typeof(ItemsSourceCountFilterConverter))]
    public class ItemsSourceCountFilterConverter : IValueConverter
    {
        public bool OrderBy { get; set; }

        public ItemsSourceCountFilterConverter()
        {
            try
            {
                this.OrderBy = true;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                var val = value as IEnumerable;
                if (val == null)
                    return value;

                int take = 10;
                if (parameter != null)
                    int.TryParse(parameter as string, out take);


                if (take < 1)
                    return value;
                var list = new List<object>();

                int count = 0;

                foreach (var li in val)
                {
                    count++;
                    if (count > take)
                        break;
                    list.Add(li);
                }
                return list;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

using LogModule;

namespace ValueConverters
{
    using ProberInterfaces.Param;
    using System;
    using System.Collections;
    using System.Globalization;
    using System.Windows.Data;

    public class GetCatCoordZValFromArrayConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                object retVal = null;

                if (value is IList && parameter != null)
                {
                    int idx = 0;
                    int tmpIdx = 0;

                    if (int.TryParse(parameter?.ToString(), out tmpIdx))
                    {
                        idx = tmpIdx;
                    }

                    IList listVal = value as IList;

                    if (idx < listVal.Count)
                    {
                        retVal = listVal[idx];
                        CatCoordinates tmp = retVal as CatCoordinates;
                        retVal = tmp?.Z.Value;
                    }
                }

                return retVal;
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


    public class NumIncreaseConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                int retVal = 0;

                if (value is int && parameter != null)
                {
                    int parseResult = 0;

                    if (int.TryParse(parameter?.ToString(), out parseResult))
                    {
                        retVal = ((int)value) + parseResult;
                    }
                }

                return retVal;
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

using LogModule;
using ProberInterfaces;
using ProberInterfaces.LoaderController;
using System;
using System.Globalization;
using System.Windows.Data;

namespace ValueConverters.Bool
{
    public class MainMenuStateConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            bool retVal = true;

            try
            {
                if(2 <= values.Length)
                {
                    if(values[0] is ModuleStateEnum && values[1] is ModuleStateEnum)
                    {
                        ModuleStateEnum lotModuleState = (ModuleStateEnum)values[0];
                        ModuleStateEnum loaderModuleState = (ModuleStateEnum)values[1];

                        if(lotModuleState == ModuleStateEnum.IDLE &&
                                (loaderModuleState != ModuleStateEnum.IDLE &&
                                 loaderModuleState != ModuleStateEnum.PAUSED &&
                                 loaderModuleState != ModuleStateEnum.ERROR))
                        {
                            retVal = false;
                        }

                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                return true;
            }

            return retVal;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

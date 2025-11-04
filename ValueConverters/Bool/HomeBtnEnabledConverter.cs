using LogModule;
using ProberInterfaces;
using ProberInterfaces.LoaderController;
using System;
using System.Globalization;
using System.Windows.Data;

namespace ValueConverters
{
    public class HomeBtnEnabledConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            bool retVal = false;

            try
            {
                if (values[0] is ModuleStateEnum && values[1] is ModuleStateEnum && values[2] is int && values[3] is bool)
                {
                    ModuleStateEnum lotModuleState = (ModuleStateEnum)values[0];
                    ModuleStateEnum loaderModuleState = (ModuleStateEnum)values[1];
                    int count = (int)values[2];
                    bool HasLock = (bool)values[3]; // True => Lock 해야 함. => IsEnabled를 False로

                    if (HasLock)
                        retVal = false;
                    else
                    {

                        if (count > 0)
                        {
                            if (
                                (lotModuleState == ModuleStateEnum.IDLE) &&
                                (loaderModuleState != ModuleStateEnum.IDLE) &&
                                (loaderModuleState != ModuleStateEnum.PAUSED) &&
                                (loaderModuleState != ModuleStateEnum.ERROR)
                            )
                            {
                                retVal = false;
                            }
                            else
                            {
                                retVal = true;
                            }
                        }
                        else
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

    public class MainMenuEnabledConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            bool retVal = true;

            try
            {
                if (values[0] is ModuleStateEnum && values[1] is ModuleStateEnum && values[2] is bool && values[3] is bool)
                {
                    ModuleStateEnum lotModuleState = (ModuleStateEnum)values[0];
                    ModuleStateEnum loaderModuleState = (ModuleStateEnum)values[1];
                    bool IsSystemError = (bool)values[2];
                    bool HasLock = (bool)values[3]; // True => Lock 해야 함.

                    if (IsSystemError == true)
                    {
                        retVal = true;
                    }
                    else if(loaderModuleState == ModuleStateEnum.RECOVERY)
                    {
                        retVal = true;
                    }
                    else
                    {
                        if (
                            (lotModuleState == ModuleStateEnum.IDLE) &&
                            (loaderModuleState != ModuleStateEnum.IDLE) &&
                            (loaderModuleState != ModuleStateEnum.PAUSED) &&
                            (loaderModuleState != ModuleStateEnum.ERROR)
                            )
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

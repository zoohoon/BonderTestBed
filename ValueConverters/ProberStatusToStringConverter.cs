using LogModule;
using ProberInterfaces;
using System;
using System.Globalization;
using System.Windows.Data;

namespace ValueConverters
{
    public class ProberStatusToStringConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            string retVal = string.Empty;

            try
            {

                //< Binding Path = "LotOPModule.ModuleState.State" />
                //< Binding Path = "LoaderController.ModuleState.State" />
                //< Binding Path = "MonitoringManager.IsSystemError" />

                if (values[0] is ModuleStateEnum && values[1] is ModuleStateEnum && values[2] is bool)
                {
                    ModuleStateEnum lotModuleState = (ModuleStateEnum)values[0];
                    ModuleStateEnum loaderModuleState = (ModuleStateEnum)values[1];
                    bool IsSystemError = (bool)values[2];

                    if (IsSystemError == true)
                    {
                        retVal = "ERROR";
                    }
                    else
                    {
                        switch (lotModuleState)
                        {
                            case ModuleStateEnum.IDLE:
                                retVal = "IDLE";
                                break;
                            case ModuleStateEnum.PAUSED:
                                retVal = "PAUSED";
                                break;
                            case ModuleStateEnum.RUNNING:
                                retVal = "RUNNING";
                                break;
                            case ModuleStateEnum.ERROR:
                                retVal = "ERROR";
                                break;
                            case ModuleStateEnum.ABORT:
                                retVal = "ABORT";
                                break;
                            case ModuleStateEnum.SUSPENDED:
                                retVal = "RUNNING";
                                break;
                            default:
                                retVal = "RUNNING";
                                break;
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

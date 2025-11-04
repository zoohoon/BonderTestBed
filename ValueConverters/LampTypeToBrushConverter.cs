using System;

namespace ValueConverters
{
    using LogModule;
    using ProberInterfaces;
    using ProberInterfaces.State;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Data;
    using System.Windows.Media;

    //public class LampTypeToBrushConverter : IValueConverter
    //{
    //    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    //    {
    //        EnumLampType lampType = (EnumLampType)value;


    //        Brush brush = Brushes.Gray;
    //        switch (lampType)
    //        {
    //            case EnumLampType.Red:
    //                brush = Brushes.Red;
    //                break;
    //            case EnumLampType.Yellow:
    //                brush = Brushes.Orange;
    //                break;
    //            case EnumLampType.Blue:
    //                brush = Brushes.LimeGreen;
    //                break;
    //            case EnumLampType.UNDEFINED:
    //                //THROUGH OUT
    //            default:
    //                //THROUGH OUT
    //                break;
    //        }

    //        return brush;
    //    }

    //    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    //    {
    //        throw new NotImplementedException();
    //    }
    //}

    public class LampTypeToBrushConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            Brush retval = Brushes.Gray;

            try
            {
                if (values.Length == 2)
                {
                    if (values[0] != DependencyProperty.UnsetValue &&
                       values[1] != DependencyProperty.UnsetValue)
                    {
                        EnumLampType lampType = (EnumLampType)values[0];
                        ModuleStateEnum LotModulestate = (ModuleStateEnum)values[1];

                        switch (lampType)
                        {
                            case EnumLampType.Red:
                                retval = Brushes.Red;
                                break;
                            case EnumLampType.Yellow:
                                retval = Brushes.Orange;
                                break;
                            case EnumLampType.Blue:
                                retval = Brushes.LimeGreen;
                                break;
                            case EnumLampType.UNDEFINED:

                                switch (LotModulestate)
                                {
                                    case ModuleStateEnum.UNDEFINED:
                                    case ModuleStateEnum.INIT:
                                    case ModuleStateEnum.PENDING:
                                    case ModuleStateEnum.SUSPENDED:
                                    case ModuleStateEnum.ABORT:
                                    case ModuleStateEnum.DONE:
                                    case ModuleStateEnum.PAUSED:
                                    case ModuleStateEnum.RECOVERY:
                                    case ModuleStateEnum.RESUMMING:
                                    case ModuleStateEnum.PAUSING:

                                        //THROUGH OUT

                                        break;
                                    case ModuleStateEnum.IDLE:
                                        retval = Brushes.Orange;
                                        break;
                                    case ModuleStateEnum.RUNNING:
                                        retval = Brushes.LimeGreen;
                                        break;
                                    case ModuleStateEnum.ERROR:
                                        retval = Brushes.Red;
                                        break;
                                    default:
                                        break;
                                }

                                break;

                            default:
                                //THROUGH OUT
                                break;
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class SysStateTypeToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            EnumSysState lampType = (EnumSysState)value;


            Brush brush = Brushes.Gray;
            switch (lampType)
            {
                case EnumSysState.IDLE:
                    brush = Brushes.Black;
                    break;
                case EnumSysState.SETUP:
                    brush = Brushes.Yellow;
                    break;
                case EnumSysState.LOT:
                    brush = Brushes.LimeGreen;
                    break;
                case EnumSysState.ERROR:
                //THROUGH OUT
                default:
                    //THROUGH OUT
                    break;
            }

            return brush;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

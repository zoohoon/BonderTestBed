using System;


namespace ValueConverters
{
    using ProberInterfaces;
    using ProberInterfaces.NeedleClean;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Data;
    using LogModule;
    public class ViewTargetConverter : IValueConverter , IFactoryModule
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                string side = parameter as string; // "Left" / "Right"

                if (value is IWaferObject)
                {
                    if (side == "Right")
                        return this.ViewModelManager().MapViewControlFD;
                    else
                        return this.ViewModelManager().MapViewControl;
                }
                else if (value is IDisplayPort)
                {
                    return value;
                }
                else if (value is INeedleCleanViewModel)
                {
                    if (this.ViewModelManager().NeedleCleanView is FrameworkElement fe)
                        fe.DataContext = value;

                    return this.ViewModelManager().NeedleCleanView;
                }
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

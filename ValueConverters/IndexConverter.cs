using LogModule;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;

namespace ValueConverters
{
    public class IndexConverter : IValueConverter
    {
        public object Convert(object value, Type TargetType, object parameter, CultureInfo culture)
        {
            try
            {
                ListViewItem item = (ListViewItem)value;
                ListView listView = ItemsControl.ItemsControlFromItemContainer(item) as ListView;
                int index = listView.ItemContainerGenerator.IndexFromContainer(item);

                index = index + 1;

                return index.ToString();
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

    public class ListBoxIndexConverter : IValueConverter
    {
        public object Convert(object value, Type TargetType, object parameter, CultureInfo culture)
        {
            try
            {
                ListBoxItem item = (ListBoxItem)value;
                ListBox listView = ItemsControl.ItemsControlFromItemContainer(item) as ListBox;
                int index = listView.ItemContainerGenerator.IndexFromContainer(item);

                index++;

                return index.ToString();
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                throw;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ListBoxIndexConverter2 : IValueConverter
    {
        public object Convert(object value, Type TargetType, object parameter, CultureInfo culture)
        {
            try
            {
                ListBoxItem item = (ListBoxItem)value;
                ListBox listView = ItemsControl.ItemsControlFromItemContainer(item) as ListBox;
                int index = listView.ItemContainerGenerator.IndexFromContainer(item);

                var a = listView.ItemContainerGenerator.ItemFromContainer(item);

                index++;

                return index.ToString();
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                throw;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class InspectionSetIndexConverter : IMultiValueConverter
    {
        //Visiabiliby
        //Visible = 0,
        //Hidden = 1,
        //Collapsed = 2
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            string retVal = string.Empty;
            long val = 0;
            try
            {
                if (values != null && values.Length ==3)
                {
                    long camIndex = 0;
                    long mapIndex = 0;
                    bool toggleSetIndex = false;

                    if (values[0] is long &&
                        values[1] is long &&
                        values[2] is bool)
                    {
                        camIndex = (long)values[0];
                        mapIndex = (long)values[1];
                        toggleSetIndex = (bool)values[2];

                        if (toggleSetIndex == true)
                        {
                            val = mapIndex;
                        }
                        else
                        {
                            val = camIndex;
                        }
                    }
                    else
                    {
                        val = 0;
                    }
                }
                else
                {
                    val = 0;
                }
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                val = 0;
            }

            retVal = val.ToString();
            return retVal;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }


    public class LoaderParamSettingViewIndexConverter : IValueConverter
    {
        public object Convert(object value, Type TargetType, object parameter, CultureInfo culture)
        {
            try
            {
                ListViewItem item = (ListViewItem)value;
                ListView listView = ItemsControl.ItemsControlFromItemContainer(item) as ListView;
                int index = listView.ItemContainerGenerator.IndexFromContainer(item)+1;
                return index.ToString();
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

    public class IndexConverterToInt : IValueConverter
    {
        public object Convert(object value, Type TargetType, object parameter, CultureInfo culture)
        {
            try
            {
                ListViewItem item = (ListViewItem)value;
                ListView listView = ItemsControl.ItemsControlFromItemContainer(item) as ListView;
                int index = listView.ItemContainerGenerator.IndexFromContainer(item);

                return index;
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

    public class MonitoringCheckIndexConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            List<int> retVal = new List<int>();

            try
            {
                if (values != null && values.Length == 2)
                {
                    int monitoringBehaviorIndex = 0;
                    int stageIdx = 0;

                    if (values[0] is int &&
                        values[1] is int)
                    {
                        monitoringBehaviorIndex = (int)values[0];
                        stageIdx = (int)values[1];

                        retVal.Add(monitoringBehaviorIndex);
                        retVal.Add(stageIdx);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

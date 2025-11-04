namespace ProberViewModel.View.E84
{
    using System;
    using System.Globalization;
    using System.Windows.Controls;
    using System.Windows.Data;
    using System.Windows.Media;
    using LogModule;
    using ProberInterfaces;
    /// <summary>
    /// E84ControlView.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class E84ControlView : UserControl, IMainScreenView
    {
        readonly Guid _ViewGUID = new Guid("1698b29e-f5f4-4597-8ac7-974bb4fbd79e");
        public Guid ScreenGUID { get { return _ViewGUID; } }
        public E84ControlView()
        {
            InitializeComponent();
        }
    }

    #region <remarks> Convert </remarks>
    public class SignalColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType,
                              object parameter, CultureInfo culture)
        {
            if (value is bool)
            {
                bool signalON = (bool)value;
                if (signalON)
                    return Brushes.Green;
                else
                    return Brushes.LightGray;
            }
            return Brushes.LightGray;
        }
        public object ConvertBack(object value, Type targetType,
                                  object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    public class ConnectLabelConverter : IValueConverter
    {
        public object Convert(object value, Type targetType,
                              object parameter, CultureInfo culture)
        {
            if (value is bool)
            {
                bool signalON = (bool)value;
                if (signalON)
                    return Brushes.Green;
                else
                    return Brushes.LightGray;
            }
            return Brushes.LightGray;
        }
        public object ConvertBack(object value, Type targetType,
                                  object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    public class E84ModeLabelConverter : IValueConverter
    {
        public object Convert(object value, Type targetType,
                              object parameter, CultureInfo culture)
        {
            if (value is bool)
            {
                bool signalON = (bool)value;
                if (signalON)
                    return "CONNECTED";
                else
                    return "DIS_CONNECTED";
            }
            return "DIS_CONNECTED";
        }


        public object ConvertBack(object value, Type targetType,
                                  object parameter, CultureInfo culture)
        {
            return null;
        }

    }

    public class CurrentStateLabelConverter : IValueConverter
    {
        public object Convert(object value, Type targetType,
                              object parameter, CultureInfo culture)
        {
            if (value is bool)
            {
                bool signalON = (bool)value;
                if (signalON)
                    return "CONNECTED";
                else
                    return "DIS_CONNECTED";
            }
            return "DIS_CONNECTED";
        }
        public object ConvertBack(object value, Type targetType,
                                  object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    public class ListViewConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            return values.Clone();
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    public class LoadPortCardLabelConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            string retVal = "";
            try
            {
                if ((values[0] is int) && (values[1] is E84OPModuleTypeEnum))
                {
                    E84OPModuleTypeEnum oPModuleTypeEnum = (E84OPModuleTypeEnum)values[1];
                    int foupnum = (int)values[0];
                    if(oPModuleTypeEnum == E84OPModuleTypeEnum.FOUP)
                    {
                        retVal = $"LOAD PORT #{foupnum}";
                    }
                    else if( oPModuleTypeEnum == E84OPModuleTypeEnum.CARD)
                    {
                        retVal = $"CARD BUFFER #{foupnum}";
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
            return null;
        }
    }
    #endregion
}

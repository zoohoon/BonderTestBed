using System;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace CommunicationConnectViewModule
{
    using ProberInterfaces;
    using System.Globalization;

    /// <summary>
    /// CommunicationConnectView.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class CommunicationConnectView : UserControl, IMainScreenView
    {
        readonly Guid _ViewGUID = new Guid("E255E260-E77F-900E-B463-BDA36F5E08ED");
        public Guid ScreenGUID { get { return _ViewGUID; } }
        public CommunicationConnectView()
        {
            InitializeComponent();
        }
    }


    public class ConnectContentConverter : IValueConverter
    {
        static SolidColorBrush Connected = new SolidColorBrush(Colors.Green);
        static SolidColorBrush NotConnected = new SolidColorBrush(Colors.Red);

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {

            try
            {
                if (value != null)
                {
                    if (value is bool)
                    {
                        if (((bool)value))
                            return "CONN";
                        else
                            return "DISCONN";
                    }
                }
            }
            catch (Exception err)
            {
                return null;
            }
            return Colors.White;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    public class ConnectButtonEnableConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                bool isEnable = true;
                if (value is ModuleStateEnum)
                {
                    ModuleStateEnum state = (ModuleStateEnum)value;
                    switch (state)
                    {
                        case ModuleStateEnum.ERROR:
                            isEnable = true;
                            break;
                        case ModuleStateEnum.IDLE:
                            isEnable = true;
                            break;
                        case ModuleStateEnum.RUNNING:
                            isEnable = false;
                            break;
                        case ModuleStateEnum.UNDEFINED:
                            isEnable = true;
                            break;
                        case ModuleStateEnum.PAUSING:
                            isEnable = false;
                            break;
                        case ModuleStateEnum.PAUSED:
                            isEnable = false;
                            break;
                        case ModuleStateEnum.ABORT:
                            isEnable = true;
                            break;
                        default:
                            isEnable = true;
                            break;
                    }
                }
            }
            catch (Exception err)
            {
                return null;
            }
            return Colors.White;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    public class GemIsOnlineStateToBoolConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            bool retVal = false;

            try
            {
                if (values != null
                    && parameter != null
                    && 2 == values.Length
                    )
                {
                    if (values[0] is SecsEnum_ON_OFFLINEState)
                    {
                        if ((SecsEnum_ON_OFFLINEState)values[0] == SecsEnum_ON_OFFLINEState.ONLINE)
                        {
                            if (values[1] is SecsEnum_OnlineSubState)
                            {
                                if (values[1].ToString() == parameter.ToString())
                                {
                                    retVal = true;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception err)
            {
                throw err;
            }


            return retVal;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            object[] retObj = null;
            try
            {
                SecsEnum_ON_OFFLINEState retVal0 = SecsEnum_ON_OFFLINEState.OFFLINE;
                SecsEnum_OnlineSubState retVAl1 = SecsEnum_OnlineSubState.LOCAL;

                if ((value is bool) && (parameter != null))
                {
                    bool isEnable = (bool)value;

                    if (isEnable == true)
                    {
                        bool parseResult = false;
                        retVal0 = SecsEnum_ON_OFFLINEState.ONLINE;

                        parseResult = Enum.TryParse<SecsEnum_OnlineSubState>(parameter.ToString(), out retVAl1);
                        if (parseResult != true)
                        {
                            retVAl1 = SecsEnum_OnlineSubState.LOCAL;
                        }
                    }
                    else
                    {
                        retVal0 = SecsEnum_ON_OFFLINEState.OFFLINE;
                    }
                }

                retObj = new object[] { retVal0, retVAl1 };
            }
            catch (Exception err)
            {
                throw err;
            }

            return retObj;
        }
    }

    public class ControlStateToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool retVal = false;

            try
            {
                if (value is SecsEnum_ControlState)
                {
                    if (value.ToString().Equals(parameter.ToString(), StringComparison.CurrentCultureIgnoreCase))
                    {
                        retVal = true;
                    }
                }
            }
            catch (Exception err)
            {
                throw err;
            }

            return retVal;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            SecsEnum_ControlState retVal = SecsEnum_ControlState.UNKNOWN;

            try
            {
                if (value is bool)
                {
                    bool isEnable = (bool)value;

                    if (parameter != null)
                    {
                        bool parseResult = false;
                        Type controlEnumType = typeof(SecsEnum_ControlState);

                        parseResult = Enum.TryParse<SecsEnum_ControlState>(parameter.ToString(), true, out retVal);

                        if (parseResult == false)
                        {
                            retVal = SecsEnum_ControlState.UNKNOWN;
                        }
                    }
                }
            }
            catch (Exception err)
            {
                throw err;
            }
            return retVal;
        }
    }
}

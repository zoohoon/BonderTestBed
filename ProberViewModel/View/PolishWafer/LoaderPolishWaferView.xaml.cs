using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;


namespace ProberViewModel
{
    using LoaderBase.Communication;
    using LogModule;
    using ProberInterfaces;
    using System.Globalization;
    using System.Windows.Media;

    public partial class LoaderPolishWaferView : UserControl, IMainScreenView
    {
        readonly Guid _ViewGUID = new Guid("E54FE805-133A-A76E-1EEE-E6D4DA81FFC9");
        public Guid ScreenGUID { get { return _ViewGUID; } }
        public LoaderPolishWaferView()
        {
            InitializeComponent();
        }
    }
    public class VirtualKeyBoardTextBoxConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            TextBox textBox = null;
            IElement element = null;
            if (values[0] != DependencyProperty.UnsetValue)
                textBox = (System.Windows.Controls.TextBox)values[0];
            if (values[1] != DependencyProperty.UnsetValue)
                element = (IElement)values[1];

            return (textBox, element);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
    public class NameAndIndexConverter : IValueConverter
    {
        public object Convert(object value, Type TargetType, object parameter, CultureInfo culture)
        {
            string retval = null;

            try
            {
                int index = (int)value;

                index = index + 1;

                if (parameter != null)
                {
                    retval = parameter.ToString() + " " + index.ToString();
                }
                else
                {
                    retval = "Unknown " + index.ToString();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class ManualTransferAndCleaingBtnEnableConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType,
              object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                bool strPos;

                if (values[0] is ModuleStateEnum)
                {
                    ModuleStateEnum val = (ModuleStateEnum)values[0];
                    bool val2 = (bool)values[1];

                    if (val != ModuleStateEnum.RUNNING)
                    {
                        strPos = true;
                    }
                    else
                    {
                        strPos = false;
                    }

                    if (!val2)
                    {
                        strPos = false;
                    }
                    else
                    {
                        if (values[2] is IStageObject)
                        {
                            IStageObject val3 = (IStageObject)values[2];
                            if (val3 != null)
                            {
                                if (val3.WaferStatus == EnumSubsStatus.EXIST)
                                {
                                    if (val3.WaferObj.WaferType.Value == EnumWaferType.POLISH)
                                    {
                                        strPos = false;
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    strPos = false;
                }

                return strPos;
            }
            catch (Exception err)
            {
                LoggerManager.Error($"ManualTransferAndCleaingBtnEnableConverter(): Error occurred. Err = {err.Message}");
                //LoggerManager.Exception(err);
                return false;
                //throw;
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes,
                  object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }
    public class ManualCleaingBtnEnableConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType,
              object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                bool strPos = false;

                if (values[0] is bool)
                {
                    bool val = (bool)values[0];

                    if (!val)
                    {
                        strPos = false;
                    }
                    else
                    {
                        if (values[1] is IStageObject)
                        {
                            IStageObject val2 = (IStageObject)values[1];
                            if (val2 != null)
                            {
                                if (val2.WaferStatus == EnumSubsStatus.EXIST)
                                {
                                    if (val2.WaferObj.WaferType.Value == EnumWaferType.POLISH)
                                    {
                                        strPos = true;
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    strPos = false;
                }

                return strPos;
            }
            catch (Exception err)
            {
                LoggerManager.Error($"ManualCleaingBtnEnableConverter(): Error occurred. Err = {err.Message}");
                //LoggerManager.Exception(err);
                return false;
                //throw;
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes,
                  object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }
    public class PWSubsStatusToValueConverter : IValueConverter
    {
        static SolidColorBrush LimeGreenbrush = new SolidColorBrush(Colors.LimeGreen);
        static SolidColorBrush DimGraybrush = new SolidColorBrush(Colors.DimGray);
        static SolidColorBrush Whitebrush = new SolidColorBrush(Colors.White);
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if (value is bool)
                {
                    bool status = (bool)value;
                    if (status)
                    {
                        return Whitebrush;
                    }
                    else
                    {
                        return DimGraybrush;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return LimeGreenbrush;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

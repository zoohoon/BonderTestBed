using System;
using System.Windows.Data;

namespace EdgeAdvanceSetup.View
{
    using ProberInterfaces;
    using LogModule;
    using MahApps.Metro.Controls.Dialogs;
    using ProberInterfaces.WaferAlignEX.Enum;
    using System.Globalization;

    /// <summary>
    /// EdgeStandardAdSetting.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class EdgeStandardAdSetting : CustomDialog , IPnpAdvanceSetupView
    {
        public EdgeStandardAdSetting()
        {
            InitializeComponent();
        }
    }

    //UI Converter
    public class EdgeTypeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                EnumWASubModuleEnable param = (EnumWASubModuleEnable)parameter;
                switch ((EnumWASubModuleEnable)value)
                {
                    case EnumWASubModuleEnable.ENABLE:
                        {
                            switch (param)
                            {
                                case EnumWASubModuleEnable.ENABLE:
                                    return true;
                                case EnumWASubModuleEnable.DISABLE:
                                    return false;
                                case EnumWASubModuleEnable.THETA_RETRY:
                                    return false;
                            }
                        }
                        break;
                    case EnumWASubModuleEnable.DISABLE:
                        {
                            switch (param)
                            {
                                case EnumWASubModuleEnable.ENABLE:
                                    return false;
                                case EnumWASubModuleEnable.DISABLE:
                                    return true;
                                case EnumWASubModuleEnable.THETA_RETRY:
                                    return false;
                            }
                        }
                        break;
                    case EnumWASubModuleEnable.THETA_RETRY:
                        {
                            switch (param)
                            {
                                case EnumWASubModuleEnable.ENABLE:
                                    return false;
                                case EnumWASubModuleEnable.DISABLE:
                                    return false;
                                case EnumWASubModuleEnable.THETA_RETRY:
                                    return true;
                            }
                        }
                        break;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                return value.Equals(true) ? parameter : Binding.DoNothing;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
    }
}

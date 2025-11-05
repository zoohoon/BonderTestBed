using System;
using System.Windows.Data;

namespace PinBlobThresholdAdvanceSetup.View
{
    using LogModule;
    using MahApps.Metro.Controls.Dialogs;
    using ProberInterfaces;
    using System.Globalization;

    /// <summary>
    /// BlobThresboldAdvanceSetupView.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class BlobThresboldAdvanceSetupView : CustomDialog
    {
        public BlobThresboldAdvanceSetupView()
        {
            InitializeComponent();
        }
    }

    public class ThresholdTypeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {


                EnumThresholdType param = (EnumThresholdType)parameter;
                //switch (PinBlobThresholdSetting.ThreshType)
                switch(value)
                {
                    case EnumThresholdType.AUTO:
                        {
                            switch (param)
                            {
                                case EnumThresholdType.AUTO:
                                    return true;
                                case EnumThresholdType.MANUAL:
                                    return false;
                            }
                        }
                        break;
                    case EnumThresholdType.MANUAL:
                        {
                            switch (param)
                            {
                                case EnumThresholdType.AUTO:
                                    return false;
                                case EnumThresholdType.MANUAL:
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

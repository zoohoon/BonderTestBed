using System;
using System.Globalization;
using System.Windows.Data;

namespace ValueConverters
{
    using LogModule;
    using ProberInterfaces.Enum;
    using System.Windows.Media.Imaging;

    public class SetupStateImageConvert : IValueConverter
    {
        

        public object Convert(object value, Type targetType,
              object parameter, CultureInfo culture)
        {
            string imgPath = string.Empty;
            try
            {
                if(value != null)
                {
                    EnumSetupProgressState pro = (EnumSetupProgressState)value;
                    switch (pro)
                    {
                        case EnumSetupProgressState.UNDEFINED:
                            imgPath = "pack://application:,,,/ImageResourcePack;component/Images/Warning.png";
                            break;
                        case EnumSetupProgressState.PROCESSING:
                            imgPath = "pack://application:,,,/ImageResourcePack;component/Images/WARotateUserDirectionICon.png";
                            break;
                        case EnumSetupProgressState.DONE:
                            imgPath = "pack://application:,,,/ImageResourcePack;component/Images/DoneCircle.png";
                            break;
                        case EnumSetupProgressState.IDLE:
                            imgPath = "pack://application:,,,/ImageResourcePack;component/Images/PnpIdle.png";
                            break;
                        case EnumSetupProgressState.SKIP:
                            imgPath = "pack://application:,,,/ImageResourcePack;component/Images/Skip.png";
                            break;
                        default:
                            imgPath = "pack://application:,,,/Resources/ArrowRightW.png";
                            break;
                    }                    
                }  
            }
            catch (Exception err)
            {
                //LoggerManager.Error($String.Format("Err = {0}", err.Message));
                LoggerManager.Exception(err);

                throw;
            }

            return new BitmapImage(new Uri(imgPath, UriKind.Absolute));          
        }

        public object ConvertBack(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}

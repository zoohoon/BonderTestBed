using System;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace WaferHandlingRecoveryControl.WaferHandleExplorerUC
{
    using LogModule;
    using ProberInterfaces;
    using System.Globalization;

    /// <summary>
    /// UcHandHolderIcon.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class UcHandHolderIcon : UserControl
    {
        public UcHandHolderIcon()
        {
            InitializeComponent();
        }

        private void icon_MouseDown(object sender, MouseButtonEventArgs e)
        {
            icon.Opacity = 0.5;
        }

        private void icon_MouseUp(object sender, MouseButtonEventArgs e)
        {
            icon.Opacity = 1;
        }

        private void icon_MouseLeave(object sender, MouseEventArgs e)
        {
            icon.Opacity = 1;
        }
    }
    public class ModuleStatusToImageSource : IMultiValueConverter
    {
        public object Convert(object[] value, Type targetType, object parameter, CultureInfo culture)
        {
            String imageSource = String.Empty;
            try
            {
                if (value[0] is ModuleTypeEnum == false)
                    return null;

                ModuleTypeEnum moduleType = (ModuleTypeEnum)value[0];

                if (value[1] is TransferObject == false)
                    imageSource = GetUnloadImageSource(moduleType);
                else
                    imageSource = GetLoadImageSource(moduleType);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

            return new BitmapImage(new Uri(imageSource, UriKind.Absolute));
        }
        public object[] ConvertBack(object value, Type[] targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
        private String GetLoadImageSource(ModuleTypeEnum moduleType)
        {
            string imagePath = String.Empty;
            try
            {
                switch (moduleType)
                {
                    case ModuleTypeEnum.CST:
                        imagePath = "pack://application:,,,/ImageResourcePack;component/Images/Minus.png";
                        break;
                    case ModuleTypeEnum.ARM:
                        imagePath = "pack://application:,,,/ImageResourcePack;component/Images/ARM1_W.png";
                        break;
                    case ModuleTypeEnum.PA:
                        imagePath = "pack://application:,,,/ImageResourcePack;component/Images/PreAligner_W.png";
                        break;
                    case ModuleTypeEnum.FIXEDTRAY:
                        imagePath = "pack://application:,,,/ImageResourcePack;component/Images/FixedTray_W.png";
                        break;
                    case ModuleTypeEnum.INSPECTIONTRAY:
                        imagePath = "pack://application:,,,/ImageResourcePack;component/Images/FixedTray_W.png";
                        break;
                    case ModuleTypeEnum.CHUCK:
                        imagePath = "pack://application:,,,/ImageResourcePack;component/Images/Chuck_W.png";
                        break;
                    default:
                        imagePath = "pack://application:,,,/ImageResourcePack;component/Images/Minus.png";
                        break;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return imagePath;
        }
        private String GetUnloadImageSource(ModuleTypeEnum moduleType)
        {
            string imagePath = String.Empty;
            try
            {
                switch (moduleType)
                {
                    case ModuleTypeEnum.CST:
                        imagePath = "pack://application:,,,/ImageResourcePack;component/Images/Minus.png";
                        break;
                    case ModuleTypeEnum.ARM:
                        imagePath = "pack://application:,,,/ImageResourcePack;component/Images/ARM1_NoWafer.png";
                        break;
                    case ModuleTypeEnum.PA:
                        imagePath = "pack://application:,,,/ImageResourcePack;component/Images/PreAligner_NoWafer.png";
                        break;
                    case ModuleTypeEnum.FIXEDTRAY:
                        imagePath = "pack://application:,,,/ImageResourcePack;component/Images/FixedTray_NoWafer.png";
                        break;
                    case ModuleTypeEnum.INSPECTIONTRAY:
                        imagePath = "pack://application:,,,/ImageResourcePack;component/Images/FixedTray_NoWafer.png";
                        break;
                    case ModuleTypeEnum.CHUCK:
                        imagePath = "pack://application:,,,/ImageResourcePack;component/Images/Chuck_NoWafer.png";
                        break;
                    default:
                        imagePath = "pack://application:,,,/ImageResourcePack;component/Images/Minus.png";
                        break;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return imagePath;
        }
    }
}

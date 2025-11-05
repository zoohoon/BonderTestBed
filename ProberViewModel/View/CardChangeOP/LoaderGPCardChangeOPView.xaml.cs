using LoaderMapView;
using LoaderParameters.Data;
using LogModule;
using ProberInterfaces;
using ProberInterfaces.CardChange;
using System;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace ProberViewModel
{
    /// <summary>
    /// GPCardChangeOPView.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class LoaderGPCardChangeOPView : UserControl, IMainScreenView
    {
        public LoaderGPCardChangeOPView()
        {
            InitializeComponent();
        }
        readonly Guid _ViewGUID = new Guid("4b9c1445-bf91-4118-b572-0106a03f2524");
        public Guid ScreenGUID { get { return _ViewGUID; } }
    }

    public class BoolToColor : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool toggle = (bool)value;
            return toggle ? "LightGreen" : "Red";
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // I don't think you'll need this
            throw new Exception("Can't convert back");
        }
    }
    public class BoolToExist : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool toggle = (bool)value;
            return toggle ? "Exist" : "Not Exist";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class BoolToOpen : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool toggle = (bool)value;
            return toggle ? "Open" : "Close";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
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

    public class ImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {

            if (value is StageObject)
            {
                return SetIconSoruceBitmap(Properties.Resources.Cell);
            }
            else if (value is ArmObject)
            {
                return SetIconSoruceBitmap(Properties.Resources.Arm);
            }
            else if (value is SlotObject)
            {
                return SetIconSoruceBitmap(Properties.Resources.FOUP);
            }
            else if (value is PAObject)
            {
                return SetIconSoruceBitmap(Properties.Resources.PA);
            }
            else if (value is BufferObject)
            {
                return SetIconSoruceBitmap(Properties.Resources.Buffer);
            }
            else if (value is CardBufferObject)
            {
                return SetIconSoruceBitmap(Properties.Resources.Cardbuffer);
            }
            else if (value is CardTrayObject)
            {
                return SetIconSoruceBitmap(Properties.Resources.CardTray);
            }
            else if (value is CardArmObject)
            {
                return SetIconSoruceBitmap(Properties.Resources.CardHand);
            }
            else if (value is FixedTrayInfoObject)
            {
                return SetIconSoruceBitmap(Properties.Resources.Buffer);
            }
            else if (value is InspectionTrayInfoObject)
            {
                return SetIconSoruceBitmap(Properties.Resources.Buffer);
            }
            else if (value is bool)
            {
                return SetIconSoruceBitmap(Properties.Resources.selecteicon);
            }

            return null;
        }
        public BitmapImage SetIconSoruceBitmap(Bitmap bitmap)
        {
            try
            {
                BitmapImage image = new BitmapImage();
                Application.Current.Dispatcher.Invoke(delegate
                {
                    using (MemoryStream ms = new MemoryStream())
                    {
                        (bitmap).Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                        image.BeginInit();
                        ms.Seek(0, SeekOrigin.Begin);
                        image.StreamSource = ms;
                        image.CacheOption = BitmapCacheOption.OnLoad; //stream disposed 이후에도 cache에 있는 stream을 사용할 수 있도록 함
                        image.EndInit();
                        image.StreamSource = null; //leak 요소 제거
                    }                        
                });                
                return image;
            }
            catch (Exception err)
            {
                throw err;
            }
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    public class TransferObjectLabelConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is StageObject)
            {
                return (value as StageObject).Name;
            }
            else if (value is ArmObject)
            {
                return (value as ArmObject).Name;
            }
            else if (value is SlotObject)
            {
                string str = "";
                str = $"Foup#{(value as SlotObject).FoupNumber + 1} - {(value as SlotObject).Name}";
                return str;
            }
            else if (value is FixedTrayInfoObject)
            {
                return (value as FixedTrayInfoObject).Name;
            }
            else if (value is PAObject)
            {
                return (value as PAObject).Name;
            }
            else if (value is BufferObject)
            {
                return (value as BufferObject).Name;
            }
            else if (value is CardBufferObject)
            {
                return (value as CardBufferObject).Name;
            }
            else if (value is CardTrayObject)
            {
                return (value as CardTrayObject).Name;
            }
            else if (value is CardArmObject)
            {
                return (value as CardArmObject).Name;
            }
            else if (value is InspectionTrayInfoObject)
            {
                return (value as InspectionTrayInfoObject).Name;
            }
            else if (value is bool)
            {
                return null;
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    public class DeviceTypeToNameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string retval = string.Empty;

            try
            {
                if (value is TransferObject)
                {
                    TransferObject to = value as TransferObject;

                    if (to.WaferType.Value == EnumWaferType.POLISH)
                    {
                        retval = to.PolishWaferInfo.DefineName.Value;
                    }
                    else
                    {
                        retval = to.DeviceName.Value;
                    }
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
            return null;
        }
    }

    public class PWAngleHiddenConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Visibility retval = Visibility.Collapsed;

            try
            {
                if (value is EnumWaferType)
                {
                    EnumWaferType type = (EnumWaferType)value;

                    if (type != EnumWaferType.POLISH)
                    {
                        retval = Visibility.Visible;
                    }
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
            return null;
        }
    }

    public class PWAngleVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Visibility retval = Visibility.Hidden;

            try
            {
                if (value is EnumWaferType)
                {
                    EnumWaferType type = (EnumWaferType)value;

                    if (type == EnumWaferType.POLISH)
                    {
                        retval = Visibility.Visible;
                    }
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
            return null;
        }
    }
    public class PWCurrentAngleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double retval = 0;

            try
            {
                if (value is double)
                {
                    retval = (double)value;

                    if ((double)value >= 360)
                    {
                        retval = (double)value % 360;
                    }
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
            return null;
        }
    }

    public class DockSequenceVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Visibility retval = Visibility.Collapsed;

            try
            {
                if (value is EnumCardChangeType)
                {
                    EnumCardChangeType type = (EnumCardChangeType)value;

                    if (type == EnumCardChangeType.CARRIER)
                    {
                        retval = Visibility.Visible;
                    }
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
            return null;
        }
    }
    
}

using System;
using System.Windows.Controls;

namespace ProberViewModel
{
    using LogModule;
    using ProberInterfaces;
    using ProberInterfaces.E84;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Data;

    /// <summary>
    /// LoaderHandlingView.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class LoaderHandlingView_GOP : UserControl, IMainScreenView
    {
        readonly Guid _ViewGUID = new Guid("da72dfc3-4a34-4206-b321-4bdbf074de7d");
        public Guid ScreenGUID { get { return _ViewGUID; } }
        public LoaderHandlingView_GOP()
        {
            InitializeComponent();
        }
    }

    public class E84EventBtnVisibilityConv : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Visibility retval = Visibility.Collapsed;

            try
            {
                if(value != null && value is IE84Parameters)
                {
                    var e84Param = (value as IE84Parameters);

                    foreach (var item in e84Param.E84Moduls)
                    {
                        if(item.E84_Attatched == true && item.E84OPModuleType == E84OPModuleTypeEnum.FOUP)
                        {
                            retval = Visibility.Visible;
                            return retval;
                        }
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

    //public class ListViewConverter : IMultiValueConverter
    //{
    //    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    //    {
    //        return values.Clone();
    //    }

    //    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    //    {
    //        return null;
    //    }
    //}

    //public class ImageConverter : IValueConverter
    //{
    //    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    //    {

    //        if (value is StageObject)
    //        {
    //            return SetIconSoruceBitmap(Properties.Resources.Cell);
    //        }
    //        else if (value is ArmObject)
    //        {
    //            return SetIconSoruceBitmap(Properties.Resources.Arm);
    //        }
    //        else if (value is SlotObject)
    //        {
    //            return SetIconSoruceBitmap(Properties.Resources.FOUP);
    //        }
    //        else if (value is PAObject)
    //        {
    //            return SetIconSoruceBitmap(Properties.Resources.PA);
    //        }
    //        else if (value is BufferObject)
    //        {
    //            return SetIconSoruceBitmap(Properties.Resources.Buffer);
    //        }
    //        else if (value is CardBufferObject)
    //        {
    //            return SetIconSoruceBitmap(Properties.Resources.CardHand);
    //        }
    //        else if (value is CardTrayObject)
    //        {
    //            return SetIconSoruceBitmap(Properties.Resources.CardTray);
    //        }
    //        else if (value is CardArmObject)
    //        {
    //            return SetIconSoruceBitmap(Properties.Resources.CardHand);
    //        }
    //        else if(value is FixedTrayInfoObject)
    //        {
    //            return SetIconSoruceBitmap(Properties.Resources.Buffer);
    //        }
    //        else if (value is InspectionTrayInfoObject)
    //        {
    //            return SetIconSoruceBitmap(Properties.Resources.Buffer);
    //        }
    //        else if (value is bool)
    //        {
    //            return SetIconSoruceBitmap(Properties.Resources.selecteicon);
    //        }

    //        return null;
    //    }
    //    public BitmapImage SetIconSoruceBitmap(Bitmap bitmap)
    //    {
    //        try
    //        {
    //            BitmapImage image = new BitmapImage();
    //            Application.Current.Dispatcher.Invoke(delegate
    //            {
    //                MemoryStream ms = new MemoryStream();
    //                (bitmap).Save(ms, System.Drawing.Imaging.ImageFormat.Png);
    //                image.BeginInit();
    //                ms.Seek(0, SeekOrigin.Begin);
    //                image.StreamSource = ms;
    //                image.EndInit();
    //            });
    //            return image;
    //        }
    //        catch (Exception err)
    //        {
    //            throw err;
    //        }
    //    }
    //    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    //    {
    //        return null;
    //    }
    //}

    //public class TransferObjectLabelConverter : IValueConverter
    //{
    //    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    //    {
    //        if (value is StageObject)
    //        {
    //            return (value as StageObject).Name;
    //        }
    //        else if (value is ArmObject)
    //        {
    //            return (value as ArmObject).Name;
    //        }
    //        else if (value is SlotObject)
    //        {
    //            string str = "";
    //            str = $"Foup#{(value as SlotObject).FoupNumber + 1} - {(value as SlotObject).Name}";
    //            return str;
    //        }else if(value is FixedTrayInfoObject)
    //        {
    //            return (value as FixedTrayInfoObject).Name;
    //        }
    //        else if (value is PAObject)
    //        {
    //            return (value as PAObject).Name;
    //        }
    //        else if (value is BufferObject)
    //        {
    //            return (value as BufferObject).Name;
    //        }
    //        else if (value is CardBufferObject)
    //        {
    //            return (value as CardBufferObject).Name;
    //        }
    //        else if (value is CardTrayObject)
    //        {
    //            return (value as CardTrayObject).Name;
    //        }
    //        else if (value is CardArmObject)
    //        {
    //            return (value as CardArmObject).Name;
    //        }
    //        else if (value is InspectionTrayInfoObject)
    //        {
    //            return (value as InspectionTrayInfoObject).Name;
    //        }
    //        else if (value is bool)
    //        {
    //            return null;
    //        }

    //        return null;
    //    }

    //    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    //    {
    //        return null;
    //    }
    //}
}

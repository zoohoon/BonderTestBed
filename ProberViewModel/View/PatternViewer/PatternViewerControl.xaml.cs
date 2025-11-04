namespace ProberViewModel
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;
    using System.Windows.Documents;
    using System.Windows.Input;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using System.Windows.Navigation;
    using System.Windows.Shapes;
    using ProberInterfaces;
    using System.IO;
    using System.Drawing;
    using System.Globalization;
    /// <summary>
    /// PatternViewerControl.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class PatternViewerControl : UserControl, IMainScreenView
    {
        readonly Guid _ViewGUID = new Guid("ce13cbd4-d266-4bb3-9a17-6992be982f89");
        public Guid ScreenGUID { get { return _ViewGUID; } }
        public PatternViewerControl()
        {
            InitializeComponent();
        }
    }

    public class ByteToImgConverter : IValueConverter
    {
        public object Convert(object value, Type targetType,
              object parameter, CultureInfo culture)
        {
            ImageBuffer imageBuffer = null;
            try
            {
                if (value is ImageBuffer)
                {
                    imageBuffer = (ImageBuffer)value;

                    //using (MemoryStream memoryStream = new MemoryStream(imageBuffer.Buffer))
                    //{
                    //    BitmapImage bitmapImage = new BitmapImage();

                    //    bitmapImage.BeginInit();

                    //    bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                    //    bitmapImage.StreamSource = memoryStream;

                    //    bitmapImage.EndInit();

                    //    return bitmapImage;
                    //}
                    if (imageBuffer != null)
                    {
                        //WriteableBitmap writeableBitmap = null;
                        if (imageBuffer.Buffer?.Count() != 0 && imageBuffer != null && imageBuffer.Buffer != new byte[0]
                                     && imageBuffer.SizeX != 0 && imageBuffer.SizeY != 0)
                        {
                            if (imageBuffer.Band == 1)
                            {
                                return WriteableBitmapFromArray(imageBuffer.Buffer, imageBuffer.SizeX, imageBuffer.SizeY, null);
                            }
                            else if (imageBuffer.Band == 3)
                            {
                                return WriteableBitmapFromColoredArray(imageBuffer.Buffer, imageBuffer.SizeX, imageBuffer.SizeY, null);
                            }
                            else
                                return null;
                        }
                        else
                            return null;
                    }
                }

                return null;
            }
            catch (Exception err)
            {
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            return null;
        }
        public static System.Windows.Media.Imaging.WriteableBitmap WriteableBitmapFromArray(byte[] imgarray,
            int width, int height, System.Windows.Media.Imaging.WriteableBitmap wrbitmap)
        {
            try
            {
                System.Windows.Media.PixelFormat pf = System.Windows.Media.PixelFormats.Gray8;

                int rawStride = (width * pf.BitsPerPixel + 7) / 8;

                System.Windows.Int32Rect anImageRectangle = new System.Windows.Int32Rect(0, 0, width, height);

                if (wrbitmap == null || wrbitmap.Format != pf
                   || width != wrbitmap.Width || height != wrbitmap.Height)
                {
                    wrbitmap = new System.Windows.Media.Imaging.WriteableBitmap(
                                width, height, 96, 96,
                                pf, null);
                }

                if (imgarray.Length == (wrbitmap.Width * wrbitmap.Height))
                    wrbitmap.WritePixels(anImageRectangle, imgarray, rawStride, 0);
            }
            catch (Exception err)
            {
            }
            return wrbitmap;
        }
        public static System.Windows.Media.Imaging.WriteableBitmap WriteableBitmapFromColoredArray(byte[] imgarray, int width, int height,
                                       System.Windows.Media.Imaging.WriteableBitmap wrbitmap)
        {
            try
            {

                System.Windows.Media.PixelFormat pf = System.Windows.Media.PixelFormats.Rgb24;

                int rawStride = width * ((pf.BitsPerPixel + 7) / 8);
                rawStride = width * ((pf.BitsPerPixel) / 8);

                System.Windows.Int32Rect anImageRectangle = new System.Windows.Int32Rect(0, 0, width, height);


                if (wrbitmap == null || wrbitmap.Format != pf)
                {
                    wrbitmap = new System.Windows.Media.Imaging.WriteableBitmap(
                                width, height, 96, 96,
                                pf, null);
                }


                wrbitmap.WritePixels(anImageRectangle, imgarray, rawStride, 0);
            }
            catch (Exception err)
            {
            }
            return wrbitmap;
        }
    }
}
